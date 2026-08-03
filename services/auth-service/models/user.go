package models

import "gorm.io/gorm"

type User struct {
	gorm.Model
	UserID   string `gorm:"uniqueIndex;not null"` // stable UUID used as guestId across services
	Email    string `gorm:"uniqueIndex;not null"`
	FullName string
	Role     string
}
