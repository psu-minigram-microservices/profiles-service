import uuid
from dataclasses import dataclass
from typing import Annotated

import jwt
from fastapi import Depends, HTTPException, Request, status
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer

from minigram_profile.config import JwtSettings, get_settings

_bearer_scheme = HTTPBearer(auto_error=False)


@dataclass
class CurrentUser:
    user_id: str | None
    email: str | None
    is_authenticated: bool

    @property
    def user_guid(self) -> uuid.UUID | None:
        if not self.user_id:
            return None
        try:
            return uuid.UUID(self.user_id)
        except (ValueError, AttributeError):
            return None


def _decode_token(token: str, jwt_settings: JwtSettings) -> dict:
    return jwt.decode(
        token,
        jwt_settings.secret,
        algorithms=["HS256"],
        audience=jwt_settings.audience,
        issuer=jwt_settings.issuer,
    )


def get_current_user(
    request: Request,
    credentials: Annotated[HTTPAuthorizationCredentials | None, Depends(_bearer_scheme)] = None,
) -> CurrentUser:
    if credentials is None:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Authentication is required.")

    settings = get_settings()

    try:
        claims = _decode_token(credentials.credentials, settings.jwt)
    except jwt.PyJWTError as exc:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail=str(exc) or "Invalid token.",
        ) from exc

    user_id = (
        claims.get("nameid")
        or claims.get("sub")
        or claims.get("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
    )
    email = claims.get("email") or claims.get("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")

    request.state.current_user = CurrentUser(
        user_id=str(user_id) if user_id is not None else None,
        email=str(email) if email is not None else None,
        is_authenticated=True,
    )
    return request.state.current_user
