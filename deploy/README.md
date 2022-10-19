* to setup namespace
ytt -f ns-rbac | kubectl create -f -
Note: if you did not override the SA imagePullSecret name, you need to create a registry secret called "registry-credentials".  Otherwise if you did override it, create one with the corresponding name.

* to deploy app
kubectl create -f pos-txn-monitor-app.yaml
