---
layout: post
title: "Как добавить docker runner к GitHub Actions"
date: 2020-06-02 22:00
tag:
- github
star: false
category: blog
author: yurii-hunter
description: "Как добавить docker runner к GitHub Actions"
image: https://github.blog/wp-content/uploads/2019/08/DL-V2-LinkedIn_FB.png
---
С момента покупки GitHub майкрософтом, на сайте появилось много полезных сервисов. Один из них - GitHub Actions. Сервис предоставляет набор функций для CI/CD. Дает возможность собирать проекты, запускать тесты и выполнять развертывание приложения.

GitHub дает возможность выполнять сборку как на внутренних серверах так и добавлять собственные и проводить сборку на них.

Ниже издложена инструкция по добавлению Docker droplet с [Digital Ocean](https://m.do.co/c/c12fb1e2c7e0) в качестве ранера к существующему проекту:

1. После создания дроплета `Docker 5:19.03.1~3 on 18.04` заходим на него через ssh
2. Первое, что нужно сделать - добавить пользователя, так как GitHub Runner не позволяет запустить агент от root
```
$ adduser github
```
3. Вводим пароль и заполняем информацию о пользователе или можно оставить ее пустой
4. Добавляем пользователя в группы [sudo](https://digitaloceancode.com/deploying-self-hosted-runners-for-github-actions/) и [docker](https://docs.docker.com/engine/install/linux-postinstall/)
```
$ adduser github sudo
$ sudo usermod -aG docker github
$ newgrp docker
```
5. Логинимся только что созданным пользователем
```
$ sudo su - github
```
6. Скачиваем и устанавливаем ранер, как указанно в инструкции _Settings -> Actions -> Add runner_. Имя ранера и рабочей директории можно оставить со значениями по умолчанию.
7. Чтобы запустить ранер как сервис, останавливаем текущий скрипт `Ctrl+C`
8. Устанавливаем сервиса ранера
```
$ ./svc.sh install
```
9. Запускаем ранер как сервис
```
$ ./svc.sh start
```
10. В workflow yaml файле меняем `runs-on: ubuntu-latest` на `runs-on: self-hosted`

![result](https://help.github.com/assets/images/help/settings/actions-runner-added.png)

