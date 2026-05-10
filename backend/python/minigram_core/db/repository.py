from typing import Generic, TypeVar

from sqlalchemy import Select, func, select
from sqlalchemy.ext.asyncio import AsyncSession

from minigram_core.db.base import BaseModel

TEntity = TypeVar("TEntity", bound=BaseModel)


class BaseRepository(Generic[TEntity]):
    def __init__(self, entity_type: type[TEntity], session: AsyncSession) -> None:
        self._entity_type = entity_type
        self._session = session

    @property
    def session(self) -> AsyncSession:
        return self._session

    def get(self) -> Select[tuple[TEntity]]:
        return select(self._entity_type)

    async def count(self) -> int:
        result = await self._session.execute(select(func.count()).select_from(self._entity_type))
        return int(result.scalar_one())

    async def create(self, entity: TEntity) -> None:
        self._session.add(entity)

    async def create_range(self, *entities: TEntity) -> None:
        self._session.add_all(entities)

    def update(self, entity: TEntity) -> None:
        self._session.add(entity)

    def update_range(self, *entities: TEntity) -> None:
        self._session.add_all(entities)

    async def delete(self, entity: TEntity) -> None:
        await self._session.delete(entity)

    async def delete_range(self, *entities: TEntity) -> None:
        for entity in entities:
            await self._session.delete(entity)

    async def save(self) -> None:
        await self._session.commit()
