service {
  name = "Bookings auth service"
  id = "AUTH-SERVICE"
  address = "host.docker.internal"
  port = 5000
  
  meta {
    prefix = "api/auth"
  }

  check {
    http = "http://host.docker.internal:5000/health"
    interval = "10s"
    timeout = "5s"
  }
}
