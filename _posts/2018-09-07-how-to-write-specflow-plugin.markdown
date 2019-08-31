---
title: "Как писать плагины к SpecFlow. Проект Macro.SpecFlow"
layout: post
date: 2018-09-07 12:00
image: /assets/images/2018/Macro.SpecFlow_featured.jpg
headerImage: false
tag:
- specflow
star: false
category: blog
author: yurii-hunter
description: Как писать плагины к SpecFlow. Проект Macro.SpecFlow
---
## Постановка задачи
У себя на проекте мы использем SpecFlow для написания тестов и часто возникала необходимость генерировать уникальные данные для каждого теста. Ну, например, имя товара. Если имя не будет уникальным, то вероятно ваше приложение не даст возможность добавить новую сущность, а ели и даст, то как потом понять, что сейчас работаем с новой, а не той, с прошлого запуска.

В общем захотелось писать в сценариях какое-то ключевое слово и чтоб потом оно заменялось на лету другим значением. Я называю это - макросы.
В таком случае, сценарий вида:

```gherkin
When I add an item 'Phone-_random_'
Then the item 'Phone-_random_' should be present in the list
```

должен замениться на:

```gherkin
When I add an item 'Phone-903456'
Then the item 'Phone-903456' should be present in the list
```

Самый простой и быстрый вариант - использовать Transformation. Мы даже жили некоторое время с таким решением, но оно не позволяет конвертировать в string и в object, входящим параметром должен быть любой другой тип. Некоторое время мы жили с MacroString типом, который являлся, просто, оберткой над string. Такой вариант не самый удобный и часто вызывал вопросы "Зачем иметь такой класс?". Так что решил я написать плагин, готорый бы препроцесил сценарии и заменял макросы на результат выполнения функции.

## Регистрация плагина
Есть несколько правил, которым нужно следовать, чтобы SpecFlow подхватывал ваш плагин:

* Имя сборки должно заканчиваться на _.SpecFlowPlugin_
* Должен быть добавлен атрибут _[assembly:RuntimePlugin(typeof(Plugin))]_
* Должен быть класс, который реализует интерфейс _IRuntimePlugin_
* В App.config'e тестового проекта в секции _plugins_ нужно добавить наш плагин `<add name="Macro" type="Runtime" />`

```csharp
using Macro.SpecFlowPlugin;
using Macro.SpecFlowPlugin.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.Plugins;

[assembly: RuntimePlugin(typeof(Plugin))]

namespace Macro.SpecFlowPlugin
{
    public class Plugin : IRuntimePlugin
    {
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters)
        {
            runtimePluginEvents.CustomizeGlobalDependencies += RuntimePluginEvents_CustomizeGlobalDependencies;
            runtimePluginEvents.CustomizeTestThreadDependencies += RuntimePluginEvents_CustomizeTestThreadDependencies;
        }

        private void RuntimePluginEvents_CustomizeGlobalDependencies(object sender, CustomizeGlobalDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<MacrosLoader, IMacrosLoader>();
        }

        private void RuntimePluginEvents_CustomizeTestThreadDependencies(object sender, CustomizeTestThreadDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<MacrosTestExecutionEngine, ITestExecutionEngine>();
            e.ObjectContainer.RegisterFactoryAs(() =>
            {
                IMacrosLoader loader = e.ObjectContainer.Resolve<IMacrosLoader>();
                IProcessor processor = new Processor();
                processor.MacroCollection = loader.LoadAll();
                return processor;
            });
        }
    }
}
```

__CustomizeGlobalDependencies__ - событие, которое возникает единажды, при загрузке сборки. В этом же событии мы регистрируем ITestExecutionEngine, который будет обрабатывать текст сценария и тут же мы подгружаем все сборки с макросами.

__CustomizeTestThreadDependencies__ - событие, которое возникает для каждого потока теста. В этом потоке мы будем хранить все макросы чтобы генерировать их каждый раз и не терять значения.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <section name="specFlow" type="TechTalk.SpecFlow.Configuration.ConfigurationSectionHandler, TechTalk.SpecFlow" />
  </configSections>
  <specFlow>
    <trace traceSuccessfulSteps="false" />
    <unitTestProvider name="NUnit" />
    <plugins>
      <add name="Macro" type="Runtime" />
    </plugins>
  </specFlow>
</configuration>
```

## Замена
С заменой все достаточно просто: нужно просто создать класс с методом, который будет заменять все слова, которые подпадают под регулярное выражение, на значение функции. Кроме того, значение должно храниться в классе и использоваться при следующем вызове, чтоб все замены одного выражения были эквивалентны.

```csharp
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Macro.SpecFlowPlugin
{
    internal interface IProcessor
    {
        string Process(string text);
        Dictionary<string, IMacro> MacroCollection { get; set; }
    }

    internal class Processor : IProcessor
    {
        public Dictionary<string, IMacro> MacroCollection { get; set; }
        private readonly Dictionary<string, string> _values;

        public Processor()
        {
            MacroCollection = new Dictionary<string, IMacro>();
            _values = new Dictionary<string, string>();
        }

        public string Process(string text)
        {
            foreach (var macro in MacroCollection)
            {
                var matches = Regex.Matches(text, macro.Key);
                foreach (Match match in matches)
                {
                    if (!match.Success) continue;
                    if (!_values.ContainsKey(match.Value))
                    {
                        _values.Add(match.Value, macro.Value.Process(match));
                    }
                    text = text.Replace(match.Value, _values[match.Value]);
                }
            }
            return text;
        }
    }
}
```

## Подгрузка классов
Штука в том, что SpecFlow - это плагин для VisualStudio, для которого я пишу плагин Macro.SpecFlow, для которого можно писть плагины... Так вот последние могут включены к любую сборку, эту сборку нужно указать в конфигурации и Macro.SpecFlow подгрузит все макросы.

```csharp
internal class MacrosLoader : IMacrosLoader
{
    private readonly string _directory = Path.GetDirectoryName(new Uri(typeof(Configuration).Assembly.CodeBase).LocalPath);
    public Dictionary<string, IMacro> LoadAll()
    {
        var list = new Dictionary<string, IMacro>();
        foreach (var assebly in Configuration.MacroSpecFlow.Assemblies as IEnumerable<AssemblyElement>)
        {
            var assembly = Assembly.LoadFile(GetFullPath(assebly.Name));
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass 
                    && typeof(IMacro).IsAssignableFrom(type) 
                    && type.HasCustomAttribute<PatternAttribute>())
                {
                    var value = type.GetCustomAttribute<PatternAttribute>().Value;
                    list[value] = Activator.CreateInstance(type) as IMacro;
                }
            }
        }

        return list;
    }

    private string GetFullPath(string name)
    {
        return Path.Combine(_directory, name);
    }
}
```

## Nuget и публикация плагина
Последний шаг - это публикация плагина. Для этого нужно создать nuget пакет и загрузить его на nuget.org

Для начала нужно создать nuspec файл и заполнить все соответствующия поля. У плагина есть зависимость на SpecFlow.CustomPlugin, поэтому нужно добавить dependency секцию и указать id пакета.

Код можно посмотреть на моем профиле в [GitHub](https://github.com/yurii-hunter/macro-specflow)