<!-- markdownlint-disable MD002 MD041 -->

In order for the Microsoft Graph to send notifications to your application running on your development machine you need to use a tool such as ngrok to tunnel calls from the internet to your development machine. Ngrok allows calls from the internet to be directed to your application running locally without needing to create firewall rules.

Before you continue you should have [ngrok](https://ngrok.com) installed on your development machine. If you do not have ngrok, visit the previous link for download options and instructions.

Run ngrok by executing the following from the command line:

```shell
ngrok http 5000
```

This will start ngrok and will tunnel requests from an external ngrok url to your development machine on port 5000.

Copy the https forwarding address. In the example below that would be `https://787b8292.ngrok.io`. You will need this later.

> [!IMPORTANT]
> Each time ngrok is restarted a new address will be generated and you will need to copy it again.

```shell
ngrok by @inconshreveable

Session Status                online
Account                       ???? ???? (Plan: Free)
Version                       2.3.15
Region                        United States (us)
Web Interface                 http://127.0.0.1:4040
Forwarding                    http://787b8292.ngrok.io -> http://localhost:5000
Forwarding                    https://787b8292.ngrok.io -> http://localhost:5000

Connections                   ttl     opn     rt1     rt5     p50     p90
                              0       0       0.00    0.00    0.00    0.00
```
