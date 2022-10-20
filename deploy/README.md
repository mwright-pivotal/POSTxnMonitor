# to setup namespace
`ytt -f ns-rbac | kubectl create -f -`
Note: you need to create a registry credential secret to namespace and add to defaul SA list of imagePullSecrets

# to deploy app
`kubectl create -f pos-txn-monitor-app.yaml`
