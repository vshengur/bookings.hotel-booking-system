package scheduler

import (
	"payment-service/internal/logging"
	"github.com/robfig/cron/v3"
)

type Cron struct {
	log *logging.Logger
	c   *cron.Cron
	job func()
}

func NewCron(log *logging.Logger, job func()) *Cron {
	return &Cron{log: log, c: cron.New(), job: job}
}

func (c *Cron) Start(spec string) {
	_, err := c.c.AddFunc(spec, c.job)
	if err != nil { c.log.Error("cron add", logging.F("err", err)); return }
	c.c.Start()
}

func (c *Cron) Stop() { c.c.Stop() }
