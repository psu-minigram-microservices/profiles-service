from typing import Generic, TypeVar

from pydantic import BaseModel, Field

TDto = TypeVar("TDto")


class PagedResponse(BaseModel, Generic[TDto]):
    count: int
    data: list[TDto] = Field(default_factory=list)
