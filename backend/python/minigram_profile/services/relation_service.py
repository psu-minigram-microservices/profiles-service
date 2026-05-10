import uuid

from sqlalchemy import func, select
from sqlalchemy.ext.asyncio import AsyncSession

from minigram_core.db.repository import BaseRepository
from minigram_core.dto.query import QueryParams
from minigram_core.exceptions import EntityNotFoundException
from minigram_core.utils.assertions import throw_if_null_or_empty
from minigram_profile.dto.profile_response import ProfileResponseDto
from minigram_profile.dto.relation_response import RelationResponseDto
from minigram_profile.dto.relation_type import tRelationType
from minigram_profile.dto.reply_status import tReplyStatus
from minigram_profile.extensions.mappers import profile_to_dto
from minigram_profile.models.relation import Relation
from minigram_profile.models.status import tStatus


class RelationService:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session
        self._repository: BaseRepository[Relation] = BaseRepository(Relation, session)

    async def get_all_by_status(
        self,
        profile_id: uuid.UUID,
        status: tStatus,
        type: tRelationType,
        query_params: QueryParams,
    ) -> list[ProfileResponseDto]:
        if query_params is None:
            raise ValueError("queryParams cannot be None")
        throw_if_null_or_empty(profile_id, "profileId")

        stmt = self._repository.get()

        if query_params.page is not None and query_params.per_page is not None:
            stmt = stmt.offset(query_params.page * query_params.per_page).limit(query_params.per_page)

        if type == tRelationType.Incoming:
            stmt = stmt.where(Relation.receiver_id == profile_id)
        elif type == tRelationType.Outgoing:
            stmt = stmt.where(Relation.sender_id == profile_id)

        stmt = stmt.where(Relation.status == status)

        result = await self._session.execute(stmt)
        relations = result.scalars().unique().all()

        if type == tRelationType.Outgoing:
            return [profile_to_dto(r.receiver) for r in relations]
        return [profile_to_dto(r.sender) for r in relations]

    async def count_by_status(
        self,
        profile_id: uuid.UUID,
        type: tRelationType,
        status: tStatus,
    ) -> int:
        throw_if_null_or_empty(profile_id, "profileId")

        stmt = select(func.count()).select_from(Relation)

        if type == tRelationType.Incoming:
            stmt = stmt.where(Relation.receiver_id == profile_id)
        elif type == tRelationType.Outgoing:
            stmt = stmt.where(Relation.sender_id == profile_id)

        stmt = stmt.where(Relation.status == status)

        result = await self._session.execute(stmt)
        return int(result.scalar_one())

    async def get(self, sender_id: uuid.UUID, receiver_id: uuid.UUID) -> RelationResponseDto:
        throw_if_null_or_empty(sender_id, "senderId")
        throw_if_null_or_empty(receiver_id, "receiverId")

        stmt = self._repository.get().where(
            Relation.sender_id == sender_id,
            Relation.receiver_id == receiver_id,
        )
        result = await self._session.execute(stmt)
        relation = result.scalars().first()

        if relation is None:
            raise EntityNotFoundException(Relation)

        return RelationResponseDto(
            status=relation.status,
            profile=profile_to_dto(relation.receiver),
        )

    async def send(self, sender_id: uuid.UUID, receiver_id: uuid.UUID) -> Relation:
        throw_if_null_or_empty(sender_id, "senderId")
        throw_if_null_or_empty(receiver_id, "receiverId")

        relation = Relation(
            id=uuid.uuid4(),
            sender_id=sender_id,
            receiver_id=receiver_id,
            status=tStatus.none,
        )

        await self._repository.create(relation)
        await self._repository.save()

        return relation

    async def reply(
        self,
        sender_id: uuid.UUID,
        receiver_id: uuid.UUID,
        status: tReplyStatus,
    ) -> None:
        throw_if_null_or_empty(sender_id, "senderId")
        throw_if_null_or_empty(receiver_id, "receiverId")

        stmt = self._repository.get().where(
            Relation.sender_id == sender_id,
            Relation.receiver_id == receiver_id,
        )
        result = await self._session.execute(stmt)
        relation = result.scalars().first()

        if relation is None:
            raise EntityNotFoundException(Relation)

        if relation.status != tStatus.none:
            raise ValueError(f"Cannot reply to relation with status {relation.status.value}.")

        if status == tReplyStatus.Accepted:
            relation.status = tStatus.friend

            reverse_stmt = select(func.count()).select_from(Relation).where(
                Relation.sender_id == receiver_id,
                Relation.receiver_id == sender_id,
            )
            reverse_count = (await self._session.execute(reverse_stmt)).scalar_one()

            if reverse_count == 0:
                reverse_relation = Relation(
                    id=uuid.uuid4(),
                    sender_id=receiver_id,
                    receiver_id=sender_id,
                    status=tStatus.friend,
                )
                await self._repository.create(reverse_relation)
        elif status == tReplyStatus.Blocked:
            relation.status = tStatus.blocked
        elif status == tReplyStatus.Rejected:
            await self._repository.delete(relation)

        await self._repository.save()

    async def delete(self, sender_id: uuid.UUID, receiver_id: uuid.UUID) -> None:
        throw_if_null_or_empty(sender_id, "senderId")
        throw_if_null_or_empty(receiver_id, "receiverId")

        stmt = self._repository.get().where(
            Relation.sender_id == sender_id,
            Relation.receiver_id == receiver_id,
        )
        result = await self._session.execute(stmt)
        relation = result.scalars().first()

        if relation is None:
            raise EntityNotFoundException(Relation)

        await self._repository.delete(relation)
        await self._repository.save()
