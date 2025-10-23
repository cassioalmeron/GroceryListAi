from pydantic import BaseModel, Field
from datetime import datetime
from typing import Optional


class ItemBase(BaseModel):
    """Base schema for Item"""
    description: str = Field(..., min_length=1, max_length=255, description="Item description")
    checked: bool = Field(default=False, description="Whether the item is checked")


class ItemCreate(ItemBase):
    """Schema for creating a new Item"""
    pass


class ItemUpdate(BaseModel):
    """Schema for updating an Item"""
    description: Optional[str] = Field(None, min_length=1, max_length=255)
    checked: Optional[bool] = None


class ItemResponse(ItemBase):
    """Schema for Item response"""
    id: int
    created_at: datetime
    updated_at: datetime
    
    class Config:
        from_attributes = True  # For Pydantic v2 (orm_mode in v1)


class ItemCheckedUpdate(BaseModel):
    """Schema for updating only the checked status"""
    checked: bool

