import uuid

from sqlalchemy.ext.asyncio import AsyncSession

from minigram_core.db.repository import BaseRepository
from minigram_core.dto.query import QueryParams
from minigram_core.exceptions import EntityNotFoundException
from minigram_core.utils.assertions import throw_if_null_or_empty
from minigram_profile.dto.profile_request import ProfileRequestDto
from minigram_profile.dto.profile_response import ProfileResponseDto
from minigram_profile.extensions.mappers import profile_to_dto
from minigram_profile.models.profile import Profile


class ProfileService:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session
        self._repository: BaseRepository[Profile] = BaseRepository(Profile, session)

    async def get_all(self, query_params: QueryParams) -> list[ProfileResponseDto]:
        if query_params is None:
            raise ValueError("queryParams cannot be None")

        stmt = self._repository.get()

        if query_params.page is not None and query_params.per_page is not None:
            stmt = stmt.offset(query_params.page * query_params.per_page).limit(query_params.per_page)

        result = await self._session.execute(stmt)
        return [profile_to_dto(p) for p in result.scalars().all()]

    async def get(self, id: uuid.UUID) -> Profile:
        throw_if_null_or_empty(id, "id")

        stmt = self._repository.get().where(Profile.id == id)
        result = await self._session.execute(stmt)
        profile = result.scalars().first()

        if profile is None:
            raise EntityNotFoundException(Profile, id)

        return profile

    async def get_by_user_id(self, user_id: uuid.UUID) -> Profile:
        throw_if_null_or_empty(user_id, "userId")

        stmt = self._repository.get().where(Profile.user_id == user_id)
        result = await self._session.execute(stmt)
        profile = result.scalars().first()

        if profile is None:
            raise EntityNotFoundException("Profile", user_id, field_name="UserId")

        return profile

    async def count(self) -> int:
        return await self._repository.count()

    async def create(self, user_id: uuid.UUID, dto: ProfileRequestDto) -> Profile:
        if dto is None:
            raise ValueError("dto cannot be None")
        throw_if_null_or_empty(user_id, "userId")

        profile = Profile(
            id=uuid.uuid4(),
            user_id=user_id,
            name=dto.name,
            photo_url=str(dto.photo_url) if dto.photo_url is not None else None,
        )

        await self._repository.create(profile)
        await self._repository.save()

        return profile

    async def update(self, profile: Profile, dto: ProfileRequestDto) -> None:
        if dto is None:
            raise ValueError("dto cannot be None")
        if profile is None:
            raise ValueError("profile cannot be None")

        profile.name = dto.name
        profile.photo_url = str(dto.photo_url) if dto.photo_url is not None else None

        self._repository.update(profile)
        await self._repository.save()
