import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  scenarios: {
    burst: {
      executor: 'constant-arrival-rate',
      rate: 10000, // RPS
      timeUnit: '1s',
      duration: '30s',
      preAllocatedVUs: 1000,
      maxVUs: 10000,
    }
  }
};

export default function () {
  const url = 'http://localhost:8080/payment/quote?roomId=R1&start=2025-10-10&end=2025-10-12&adults=2&children=0';
  const res = http.get(url);
  check(res, { 'status was 200': (r) => r.status === 200 });
  sleep(0.001);
}
