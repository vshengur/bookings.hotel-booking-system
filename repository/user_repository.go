package repository

import (
	"log"

	"github.com/vshengur/bookings.auth-service/models"
	"gorm.io/gorm"
)

type UserRepository interface {
	FindByEmail(email string) *models.User
	FindOrCreate(user *models.User) *models.User
}

type userRepository struct {
	db *gorm.DB
}

func NewUserRepository(db *gorm.DB) UserRepository {
	return &userRepository{db: db}
}

func (r *userRepository) FindByEmail(email string) *models.User {
	var user models.User
	err := r.db.Where("email = ?", email).First(&user).Error
	if err != nil {
		log.Printf("User not found: %v", err)
		return nil
	}
	return &user
}

func (r *userRepository) FindOrCreate(user *models.User) *models.User {
	existingUser := r.FindByEmail(user.Email)
	if existingUser != nil {
		return existingUser
	}

	err := r.db.Create(user).Error
	if err != nil {
		log.Printf("Failed to create user: %v", err)
		return nil
	}
	return user
}
