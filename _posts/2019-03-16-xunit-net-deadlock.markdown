---
title: "Зависшие тесты в xUnit.net"
layout: post
date: 2019-03-16 12:00
image: /assets/images/2019/xunit-position.png
headerImage: false
tag:
- xunit
- mstest
star: false
category: blog
author: yurii-hunter
description: Зависшие тесты в xUnit.net
---
Столкнулся с проблемой, что при запуске xUnit тестов они зависают. Причем проблема воспроизводилась не на всех машинах и не всегда. Чаще всего если запускать тесты в режиме Run All. Или если запускать тесты с помощью dotnet test команды.

[Нагуглил](https://github.com/xunit/xunit/issues/864), что проблема связанна с deadlock которые возникают если использовать `.Wait()` функцию.

Самое простое решение проблемы - выключить параллельный запуск тестов. Добавьте в любом файле проекта (обычно это Assembly.cs) `[assembly: CollectionBehavior(DisableTestParallelization = true)`

Сами ребята из xUnit - очень специфичные и говорят, что исправлять ничего не будут и такое поведение As Designed. Думаю это мантра, с которой они начинают свой день.

![](/assets/images/2019/xunit-position.png)