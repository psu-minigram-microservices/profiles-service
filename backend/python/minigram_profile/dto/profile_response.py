import uuid

from pydantic import BaseModel, ConfigDict, Field


class ProfileResponseDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True, from_attributes=True)

    id: uuid.UUID
    name: str
    photo_url: str | None = Field(default=None, alias="photoUrl")
