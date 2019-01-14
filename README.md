
# HouseTag_Web

楼盘评论标签Web版


----------
[HouseTag](https://github.com/cfan1236/HouseTag "housetag") 升级改进版

[http://housetag.580zdh.com/](http://housetag.580zdh.com/ "housetag")

HouseTag 之前开发的是Windows桌面应用程序版本；软件发布后有些看房的朋友问我有没有Mac版本及Web版；看到很多人给我反馈这个消息。这个月刚好有点时间就想着把项目进行改进下。由于本人不是专业前端所以UI在搭配和和设计上可能有点low；也不能保证每个不同大小的手机都能完美兼容。

## 升级

1、新增Web UI。

2、新增评论搜索可根据自己需要搜索评论内容。

3、新增代理IP切换功能。


### 前端
前端UI框架使用的是Layui+jQuery,兼容手机和PC；同时在Layui官网找了个开源的搜索展示列表组件yutons_sug；组件根据自己需要做一些调整;包括组件的UI。

### 后端
后端接口部分，基于HouseTag之前的代码全部用Dotnet Core 重新写了一遍。 各个模块之间的调用也都写成接口(interface)形式了， 通过依赖注入实现实例化和生命周期的管理。
### 代理
在本次的开发过程中发经过多次实验发现安居客现在对新房数据增加了反爬虫技术，不知道是不是因为我的HouseTag让他们发现流量异常了；之前确实没发现有这个滑动验证功能。即一个IP频繁请求会被服务器拦截需要进行滑动验证。IP被拦截了最好的办法就是使用代理了。因此我专门开发一个自动获取免费代理IP的定时任务：[ProxyIpSchedule](https://github.com/cfan1236/ProxyIpSchedule"housetag") 当然也是基于Dotnet Core写的。因为是免费的代理IP真正能用的可能只有1/100 甚至不到。还有免费代理IP时效性很不稳定，很有可能立马就失效了。所以在使用时最好是实时检测他的有效性。

项目还有很多不完善之处，也有很多值得继续开发的功能。后续我会继续维护和完善此项目。
线上环境如果很慢请谅解，我的服务器和带宽都是很小的配置。

交流反馈:rui1236@hotmail.com


### 项目目录

```
HouseTag
│   │
│   wwwroot
|   |   | //前端js css layui等。
│   Controllers
|   |   | //后端接口控制器。
│   Data
|   |   | //URL 标签等配置文件。
│   Helper
|   |   | //公共组件
│   Models 
|   |   | //实体类
│   IService
|   |   | //服务接口
│   Service
|   |   | //服务实现
│   Views
|   |   | //页面文件 html
│   appsettings.json
|   |   | //项目配置文件
│   Nlog.config
|   |   | //nlog 配置文件

```
### 运行截图
![图1](https://github.com/cfan1236/HouseTag_Web/blob/master/doc/img/houseTag_Web_PC.jpg)

![图2](https://github.com/cfan1236/HouseTag_Web/blob/master/doc/img/houseTag_Web_mobile.jpg)

