package models

import "gorm.io/gorm"

type User struct {
	gorm.Model
	Email    string `gorm:"uniqueIndex;not null"`
	FullName string
	Role     string
}
