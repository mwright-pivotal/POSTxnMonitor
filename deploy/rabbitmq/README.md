# Notes

 - Use the same topology options in both Edge and DC deployments of RabbitMQCLusters (edge-pos-messaging)
 - Deploy DC rabbit cluster first, obtain the assigned LB IP and username/password from generated secret (edge-pos-messaging-default-user)
 - Deploy Edge rabbit clusters next.
 - Deploy app to both DC and Edge, ideally in the same ns as rabbit to keep things simple.  We will mount edge-pos-messaging-default-user in the app deployment to get access to local RMQ.
 - Don't use Shovel yaml to enable Shovel (need to followup on correct config)
 - Exec into RMQ pod running at edge to enable Shovel
 `kubectl -n pos-edge exec -it edge-pos-messaging-server-0 bash`
 `rabbitmqctl set_parameter shovel my-shovel   '{"src-protocol": "amqp091", "src-uri": "amqp://", "src-queue": "my-reliable-pos-txns", "dest-protocol": "amqp091", "dest-uri": "amqp://<dc-rmq-user>:<dc-rmq-pswd>@<dc-lb-ip-rmq-service>", "dest-queue": "my-reliable-pos-txns"}'`