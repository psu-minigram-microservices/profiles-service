import logging
import traceback
from http import HTTPStatus

from fastapi import FastAPI, HTTPException, Request
from fastapi.exceptions import RequestValidationError
from fastapi.responses import JSONResponse
from sqlalchemy.exc import IntegrityError, SQLAlchemyError

from minigram_core.dto.error import ErrorResponse
from minigram_core.exceptions import EntityNotFoundException

logger = logging.getLogger(__name__)

DEFAULT_ERROR_MESSAGE = "An unexpected error occurred. Please try again later."

ERROR_MESSAGES: dict[int, str] = {
    HTTPStatus.NOT_FOUND: "The requested resource was not found.",
    HTTPStatus.UNAUTHORIZED: "Authentication is required to access this resource.",
    HTTPStatus.BAD_REQUEST: "The request contains invalid data.",
    HTTPStatus.CONFLICT: "A conflict occurred while processing your request. The resource may have been modified.",
    HTTPStatus.FORBIDDEN: "Do not have permission to access this resource.",
}


def _build_response(status_code: int, exception: BaseException, is_development: bool) -> JSONResponse:
    if is_development:
        message = str(exception) or ERROR_MESSAGES.get(status_code, DEFAULT_ERROR_MESSAGE)
    else:
        message = ERROR_MESSAGES.get(status_code, DEFAULT_ERROR_MESSAGE)

    body = ErrorResponse(status_code=status_code, message=message)

    if is_development:
        body.exception_type = type(exception).__name__
        body.stack_trace = "".join(traceback.format_exception(exception))

    return JSONResponse(
        status_code=status_code,
        content=body.model_dump(by_alias=False, exclude_none=True),
    )


def register_exception_handlers(app: FastAPI, *, is_development: bool) -> None:
    @app.exception_handler(EntityNotFoundException)
    async def _entity_not_found(_: Request, exc: EntityNotFoundException) -> JSONResponse:
        return _build_response(HTTPStatus.NOT_FOUND, exc, is_development)

    @app.exception_handler(PermissionError)
    async def _forbidden(_: Request, exc: PermissionError) -> JSONResponse:
        return _build_response(HTTPStatus.FORBIDDEN, exc, is_development)

    @app.exception_handler(ValueError)
    async def _bad_request(_: Request, exc: ValueError) -> JSONResponse:
        return _build_response(HTTPStatus.BAD_REQUEST, exc, is_development)

    @app.exception_handler(RequestValidationError)
    async def _validation_error(_: Request, exc: RequestValidationError) -> JSONResponse:
        return _build_response(HTTPStatus.BAD_REQUEST, exc, is_development)

    @app.exception_handler(IntegrityError)
    async def _integrity_error(request: Request, exc: IntegrityError) -> JSONResponse:
        logger.error("Database integrity error on %s %s", request.method, request.url.path, exc_info=exc)
        return _build_response(HTTPStatus.CONFLICT, exc, is_development)

    @app.exception_handler(SQLAlchemyError)
    async def _sqlalchemy_error(request: Request, exc: SQLAlchemyError) -> JSONResponse:
        logger.error("Database error on %s %s", request.method, request.url.path, exc_info=exc)
        return _build_response(HTTPStatus.CONFLICT, exc, is_development)

    @app.exception_handler(HTTPException)
    async def _http_exception(_: Request, exc: HTTPException) -> JSONResponse:
        return _build_response(exc.status_code, exc, is_development)

    @app.exception_handler(Exception)
    async def _unhandled(request: Request, exc: Exception) -> JSONResponse:
        logger.error("Unhandled exception on %s %s", request.method, request.url.path, exc_info=exc)
        return _build_response(HTTPStatus.INTERNAL_SERVER_ERROR, exc, is_development)
