from pydantic import AnyUrl, BaseModel, ConfigDict, Field


class ProfileRequestDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    name: str = Field(..., min_length=1)
    photo_url: AnyUrl | None = Field(default=None, alias="photoUrl")
