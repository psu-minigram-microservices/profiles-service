import uuid
from typing import Annotated

from fastapi import APIRouter, Depends, HTTPException, status

from minigram_core.dto.paged import PagedResponse
from minigram_core.dto.query import QueryParams
from minigram_core.exceptions import EntityNotFoundException
from minigram_profile.dependencies import ProfileServiceDep
from minigram_profile.dto.profile_request import ProfileRequestDto
from minigram_profile.dto.profile_response import ProfileResponseDto
from minigram_profile.extensions.mappers import profile_to_dto
from minigram_profile.services.current_user import CurrentUser, get_current_user

router = APIRouter(prefix="/profiles", tags=["Profile"])


def _user_id(current_user: CurrentUser) -> uuid.UUID:
    user_guid = current_user.user_guid
    if user_guid is None:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Authentication is required.")
    return user_guid


@router.get("", response_model=PagedResponse[ProfileResponseDto])
async def get_all(
    profile_service: ProfileServiceDep,
    query_params: Annotated[QueryParams, Depends()],
    _: Annotated[CurrentUser, Depends(get_current_user)],
) -> PagedResponse[ProfileResponseDto]:
    count = await profile_service.count()
    data = await profile_service.get_all(query_params)
    return PagedResponse[ProfileResponseDto](count=count, data=data)


@router.get("/me", response_model=ProfileResponseDto)
async def me(
    profile_service: ProfileServiceDep,
    current_user: Annotated[CurrentUser, Depends(get_current_user)],
) -> ProfileResponseDto:
    profile = await profile_service.get_by_user_id(_user_id(current_user))
    return profile_to_dto(profile)


@router.get("/{id}", response_model=ProfileResponseDto)
async def get(
    id: uuid.UUID,
    profile_service: ProfileServiceDep,
    _: Annotated[CurrentUser, Depends(get_current_user)],
) -> ProfileResponseDto:
    try:
        profile = await profile_service.get(id)
    except EntityNotFoundException:
        profile = await profile_service.get_by_user_id(id)
    return profile_to_dto(profile)


@router.post("", response_model=ProfileResponseDto, status_code=status.HTTP_201_CREATED)
async def create(
    dto: ProfileRequestDto,
    profile_service: ProfileServiceDep,
    current_user: Annotated[CurrentUser, Depends(get_current_user)],
) -> ProfileResponseDto:
    profile = await profile_service.create(_user_id(current_user), dto)
    return profile_to_dto(profile)


@router.put("", response_model=ProfileResponseDto, status_code=status.HTTP_201_CREATED)
async def update(
    dto: ProfileRequestDto,
    profile_service: ProfileServiceDep,
    current_user: Annotated[CurrentUser, Depends(get_current_user)],
) -> ProfileResponseDto:
    profile = await profile_service.get_by_user_id(_user_id(current_user))
    await profile_service.update(profile, dto)
    return profile_to_dto(profile)
