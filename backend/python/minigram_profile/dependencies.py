from collections.abc import AsyncIterator
from typing import Annotated

from fastapi import Depends, Request
from sqlalchemy.ext.asyncio import AsyncSession

from minigram_core.db.session import Database
from minigram_profile.services.profile_service import ProfileService
from minigram_profile.services.relation_service import RelationService


async def get_session(request: Request) -> AsyncIterator[AsyncSession]:
    database: Database = request.app.state.database
    async for session in database.session():
        yield session


SessionDep = Annotated[AsyncSession, Depends(get_session)]


def get_profile_service(session: SessionDep) -> ProfileService:
    return ProfileService(session)


def get_relation_service(session: SessionDep) -> RelationService:
    return RelationService(session)


ProfileServiceDep = Annotated[ProfileService, Depends(get_profile_service)]
RelationServiceDep = Annotated[RelationService, Depends(get_relation_service)]
