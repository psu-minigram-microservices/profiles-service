import uuid
from typing import Annotated

from fastapi import APIRouter, Depends, HTTPException, Query, Response, status

from minigram_core.dto.paged import PagedResponse
from minigram_core.dto.query import QueryParams
from minigram_profile.dependencies import ProfileServiceDep, RelationServiceDep
from minigram_profile.dto.profile_response import ProfileResponseDto
from minigram_profile.dto.relation_response import RelationResponseDto
from minigram_profile.dto.relation_type import tRelationType
from minigram_profile.dto.reply_status import tReplyStatus
from minigram_profile.models.status import tStatus
from minigram_profile.services.current_user import CurrentUser, get_current_user

router = APIRouter(prefix="/profiles/relations", tags=["Relation"])


def _user_id(current_user: CurrentUser) -> uuid.UUID:
    user_guid = current_user.user_guid
    if user_guid is None:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Authentication is required.")
    return user_guid


@router.get("", response_model=PagedResponse[ProfileResponseDto])
async def get_by_status(
    profile_service: ProfileServiceDep,
    relation_service: RelationServiceDep,
    query_params: Annotated[QueryParams, Depends()],
    current_user: Annotated[CurrentUser, Depends(get_current_user)],
    status: Annotated[tStatus, Query(...)],
    type: Annotated[tRelationType, Query(...)],
) -> PagedResponse[ProfileResponseDto]:
    profile = await profile_service.get_by_user_id(_user_id(current_user))
    count = await relation_service.count_by_status(profile.id, type, status)
    data = await relation_service.get_all_by_status(profile.id, status, type, query_params)
    return PagedResponse[ProfileResponseDto](count=count, data=data)


@router.post("/send/{receiverid}", status_code=status.HTTP_201_CREATED)
async def send(
    profile_service: ProfileServiceDep,
    relation_service: RelationServiceDep,
    current_user: Annotated[CurrentUser, Depends(get_current_user)],
    receiverid: uuid.UUID,
) -> Response:
    profile = await profile_service.get_by_user_id(_user_id(current_user))
    await relation_service.send(profile.id, receiverid)
    return Response(status_code=status.HTTP_201_CREATED)


@router.post("/reply/{senderid}", status_code=status.HTTP_204_NO_CONTENT)
async def reply(
    profile_service: ProfileServiceDep,
    relation_service: RelationServiceDep,
    current_user: Annotated[CurrentUser, Depends(get_current_user)],
    senderid: uuid.UUID,
    status: Annotated[tReplyStatus, Query(...)],
) -> Response:
    profile = await profile_service.get_by_user_id(_user_id(current_user))
    await relation_service.reply(senderid, profile.id, status)
    return Response(status_code=204)


@router.get("/{receiverid}", response_model=RelationResponseDto)
async def get(
    profile_service: ProfileServiceDep,
    relation_service: RelationServiceDep,
    current_user: Annotated[CurrentUser, Depends(get_current_user)],
    type: Annotated[tRelationType, Query(...)],
    receiverid: uuid.UUID,
) -> RelationResponseDto:
    profile = await profile_service.get_by_user_id(_user_id(current_user))

    if type == tRelationType.Outgoing:
        return await relation_service.get(profile.id, receiverid)
    return await relation_service.get(receiverid, profile.id)


@router.delete("/{receiverid}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_relation(
    profile_service: ProfileServiceDep,
    relation_service: RelationServiceDep,
    current_user: Annotated[CurrentUser, Depends(get_current_user)],
    receiverid: uuid.UUID,
) -> Response:
    profile = await profile_service.get_by_user_id(_user_id(current_user))
    await relation_service.delete(profile.id, receiverid)
    return Response(status_code=204)
