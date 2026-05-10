from pydantic import BaseModel, ConfigDict


class ErrorResponse(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    status_code: int
    message: str
    exception_type: str | None = None
    stack_trace: str | None = None
