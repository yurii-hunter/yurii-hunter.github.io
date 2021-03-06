---
layout: post
title: "Как подключить Chatbase аналитику к чатботу"
date: 2020-11-08 12:00
tag:
- telegram
- chatbase
- python
star: false
category: blog
author: yurii-hunter
description: "Как подключить Chatbase аналитику к чатботу. Аналог Google Analytics для Telegram ботов"
image: /assets/images/2020/chatbase.png
---
Сколько пользователей у чат бота, какой функционал самый используемый и на каком этапе воронки отваливаются пользователи? На эти и другие вопросы поможет ответить сервис аналитики для чат ботов. Рекомендую [Chatbase](https://chatbase.com/). О том как его подключить и как настроить, отвечу в этой статье.

## Что такое Chatbase?
Сервис аналитики для чат ботов. Отслеживает колличество Active Users, Sessions, Retention и Funnels. После регистрации создается демо проет, на котором можно ознакомиться с функционалом. 

С одной стороны - это проект Google. Клиентские библиотеки размещены в репозитории google на GitHub, но с другой стороны в том же репозитории разместилась ремарка.
> This is not an official Google product

В любом рекомендую попробовать. Проект пока бесплатный и интеграция не занимает много времени.

## Как установить?
В зависимости от платформы, с которой вы работаете нужно установить соответствующую библиотеку. Пока доступны для [Dotnet](https://github.com/google/chatbase-dotnet), [Node.js](https://github.com/google/chatbase-node) и [Python](https://github.com/google/chatbase-python). У Chatbase открытый и простой API, так что если вы пишите на другом языке, то реализовать его вам не составит особого труда.

Я для своих проектов использую Python. Установим пакет с github
```
> pip install git+https://github.com/google/chatbase-python.git#egg=chatbase
```

## Основная концепция
Основная задача Chatbase - отслеживать диалог между пользователем и ботом. Находить сообщения, которые не были распознаны и отображать их в консоле. 
Поэтому у сообщений бывает двух типов: `user` - для сообщений, которые пользователь прислал боту, `agent` - для тех сообщений, которые отправил бот в ответ.

## Как отправлять запрос
Импортируем класс Message с пакета chatbase

```python
from chatbase import Message
```

Создаем сообщение и отправляем

```python
message = Message(
        api_key='api_key - xxx', # Chatbase ID бота
        platform='Telegram',
        message='Text message', # сообощение, которое прислал пользователь или ответ бота
        intent='intent', # название диалога. Например: регистрация или добавление товара
        version='1.0', # версия твоего бота
        user_id='123456' # ID пользователя телеграм
    )
message.set_as_type_user() # если логируем сообщение пользователя
# message.set_as_type_agent() # если логируем ответ от бота
message.send()
```

## Обработка сообщения
После отправки сообщение будет обработано на сервере и отобразится на графиках. Обработка может занять от нескольких минут до нескольких часов. Не спеши исправлять код, если сообщение не появилось сразу.

Как настроить воронку или просмотреть список всех сообщений я опишу в следующей статье.