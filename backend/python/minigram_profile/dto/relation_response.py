from pydantic import BaseModel, ConfigDict

from minigram_profile.dto.profile_response import ProfileResponseDto
from minigram_profile.models.status import tStatus


class RelationResponseDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    status: tStatus
    profile: ProfileResponseDto
