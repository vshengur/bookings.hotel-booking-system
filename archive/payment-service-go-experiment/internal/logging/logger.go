package logging

import "go.uber.org/zap"

type Logger struct {
	*zap.Logger
}

func NewLogger(env string) *Logger {
	cfg := zap.NewProductionConfig()
	if env == "dev" {
		cfg = zap.NewDevelopmentConfig()
	}
	l, _ := cfg.Build()
	return &Logger{l}
}

func (l *Logger) Info(msg string, fields ...zap.Field)  { l.Logger.Info(msg, fields...) }
func (l *Logger) Error(msg string, fields ...zap.Field) { l.Logger.Error(msg, fields...) }
func (l *Logger) Fatal(msg string, fields ...zap.Field) { l.Logger.Fatal(msg, fields...) }

func F(k string, v any) zap.Field { return zap.Any(k, v) }
