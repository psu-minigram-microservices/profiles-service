from pydantic import BaseModel, ConfigDict, field_serializer

from minigram_profile.dto.profile_response import ProfileResponseDto
from minigram_profile.models.status import tStatus


class RelationResponseDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    status: tStatus
    profile: ProfileResponseDto

    @field_serializer("status")
    def _serialize_status(self, value: tStatus) -> str:
        return value.value.capitalize()
