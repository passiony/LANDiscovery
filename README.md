# SimpleDiscovery for Unity

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Platform: Android / Windows](https://img.shields.io/badge/Platform-Android%20%7C%20Windows-blue.svg)]()

一个专为 **Unity** 打造的、超轻量级局域网发现（LAN Discovery）组件。

## 🚀 为什么选择 SimpleDiscovery？

在开发 Android (Pico/Quest) 或 Windows 平台的局域网多人游戏时，官方的 Discovery 扩展库经常会遇到搜不到 IP 的问题。本项目通过剥离复杂框架，回归底层 UDP 通信，解决了以下顽疾：

- **Android 组播锁 (MulticastLock)**：自动处理 Android 系统对 UDP 广播的拦截。
- **防火墙兼容性**：针对 Windows 防火墙策略优化的同步回显逻辑。
- **多网卡干扰**：自动计算子网掩码，执行子网定向广播，避开虚拟网卡（如 VMware/VPN）的干扰。
- **零依赖**：不依赖 Mirror 或 LiteNetLib，仅使用原生 C# Socket 实现。

---

## 🛠 功能特性

- [x] **一键发现**：自动获取局域网内所有运行中的服务器。
- [x] **跨平台支持**：完美支持 PC (Windows) 和 VR 移动端 (Pico/Quest)。
- [x] **解耦设计**：只需获取 IP 字符串，即可轻松对接 NGO 的 `UnityTransport`。
- [x] **持续敲门**：客户端循环广播机制，应对移动端网卡唤醒延迟。

---

## 📦 快速上手

### 1. 导入脚本
将 `SimpleDiscovery.cs` 放入你的 Unity 项目 `Assets/Scripts` 文件夹中。

### 2. 服务器端配置 (Host/Server)
```csharp
public SimpleDiscovery discovery;

void StartHost() 
{
    NetworkManager.Singleton.StartHost();
    // 启动发现服务，可以传入自定义数据（如房间名）
    discovery.StartServer("MyAwesomeRoom");
}
public SimpleDiscovery discovery;
```

3. 客户端配置 (Client)
```csharp
void StartSearching() 
{
    discovery.FindServers((ip, data) => {
        Debug.Log($"Found Server at: {ip}, Data: {data}");
        
        // 对接 NGO 的 UnityTransport
        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        transport.SetConnectionData(ip, 7777); 
        
        // 连接服务器
        NetworkManager.Singleton.StartClient();
        
        // 停止搜索
        discovery.StopDiscovery();
    });
}
```
