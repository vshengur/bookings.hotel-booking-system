package metrics

import "github.com/prometheus/client_golang/prometheus/promhttp"
import "net/http"

func Handler() http.Handler { return promhttp.Handler() }
