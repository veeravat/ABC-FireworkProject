from locust import task, between
from locust.contrib.fasthttp import FastHttpUser
import random
import string
import resource
resource.setrlimit(resource.RLIMIT_NOFILE, (999999, 999999))


class MyUser(FastHttpUser):
    wait_time = between(2, 5)

    @task
    def index(self):
        response = self.client.post("/Firework", json={
            "Name": genId.id_generator()
        })
        response = self.client.get("/Firework")


class genId():
    def id_generator(size=6, chars=string.ascii_uppercase + string.digits):
        return ''.join(random.choice(chars) for _ in range(size))
