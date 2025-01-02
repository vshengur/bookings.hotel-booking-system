package repository

import (
	"log"

	"github.com/vshengur/bookings.auth-service/models"
	"gorm.io/gorm"
)

type UserRepository interface {
	FindByEmail(email string) *models.User
	FindOrCreate(user *models.User) *models.User

	GetAllUsers() []models.User
	GetUserByID(id uint) *models.User
	CreateUser(user *models.User) *models.User
	UpdateUser(id uint, updatedUser *models.User) *models.User
	DeleteUser(id uint) bool
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

// Получение всех пользователей
func (r *userRepository) GetAllUsers() []models.User {
	var users []models.User
	if err := r.db.Find(&users).Error; err != nil {
		log.Printf("Error fetching users: %v", err)
		return nil
	}
	return users
}

// Получение пользователя по ID
func (r *userRepository) GetUserByID(id uint) *models.User {
	var user models.User
	if err := r.db.First(&user, id).Error; err != nil {
		log.Printf("User not found with ID %d: %v", id, err)
		return nil
	}
	return &user
}

// Создание нового пользователя
func (r *userRepository) CreateUser(user *models.User) *models.User {
	if err := r.db.Create(user).Error; err != nil {
		log.Printf("Error creating user: %v", err)
		return nil
	}
	return user
}

// Обновление пользователя
func (r *userRepository) UpdateUser(id uint, updatedUser *models.User) *models.User {
	existingUser := r.GetUserByID(id)
	if existingUser == nil {
		return nil
	}

	existingUser.Email = updatedUser.Email
	existingUser.FullName = updatedUser.FullName
	existingUser.Role = updatedUser.Role

	if err := r.db.Save(existingUser).Error; err != nil {
		log.Printf("Error updating user with ID %d: %v", id, err)
		return nil
	}
	return existingUser
}

// Удаление пользователя
func (r *userRepository) DeleteUser(id uint) bool {
	if err := r.db.Delete(&models.User{}, id).Error; err != nil {
		log.Printf("Error deleting user with ID %d: %v", id, err)
		return false
	}
	return true
}
