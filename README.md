# 🎯 OrdexIn

> **Gestión de inventarios y punto de venta para pequeñas operaciones.**

![.NET](https://img.shields.io/badge/.NET-10.0-purple?style=for-the-badge&logo=.net)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-blue?style=for-the-badge&logo=dot-net)
![SignalR](https://img.shields.io/badge/SignalR-realtime-green?style=for-the-badge)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5-blueviolet?style=for-the-badge&logo=bootstrap)


## 📖 Manual de uso

disponible en [docs/ManualUso.md](docs/ManualUso.md)

---

## Presentación

Precentación para defensa del proyecto disponible en [Presentación](https://www.canva.com/design/DAG6bHU-s-E/FWbqz1Xqfy1mrcquJ6aKgg/view?utm_content=DAG6bHU-s-E&utm_campaign=designshare&utm_medium=link2&utm_source=uniquelinks&utlId=h170c82609a)

---

## 📄 Descripción

`OrdexIn` es una aplicación web basada en ASP.NET Core MVC diseñada para administrar catálogo de productos, lotes, kardex (movimientos de inventario) y un módulo de punto de venta (POS). Soporta actualizaciones en tiempo real mediante SignalR. Autenticación externa y persistencia en Supabase (PosgradeSQL).

---

## 👥 Equipo de desarrollo

<!--- tabla de miembros del equipo --->
| Nombre                | Rol Principal                                                  | Responsabilidades Clave                                                                                                                                                                                                                                               | GitHub                                                                                                                                                   |
|:----------------------|:---------------------------------------------------------------|:----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|:---------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Bismarck Flores**   | **Desarrollador Principal y Full-Stack (Líder Técnico)**       | - **Integración:** Conexión y sincronización Backend-Frontend.<br/>- **Tiempo Real:** Implementación y configuración de **SignalR**. <br/>- **Datos y Seguridad (Supabase):** Creación de Tablas, Reglas **RLS**, e implementación de **Autenticación con Cookies**.  | [![GitHub Badge](https://img.shields.io/badge/GitHub-BismarckFlores-100000?style=flat&logo=github&logoColor=white)](https://github.com/BismarckFlores)   |
| **Diego Gómez**       | **Desarrollador Backend (Especialista en Módulos de Negocio)** | - Desarrollo de la lógica del **Backend** para el módulo de **Inventario**. <br/>- Desarrollo de la lógica del **Backend** para el módulo de **Punto de Venta**.                                                                                                      | [![GitHub Badge](https://img.shields.io/badge/GitHub-DiegoAGomezS-100000?style=flat&logo=github&logoColor=white)](https://github.com/DiegoAGomezS)   |
| **Anthony González**  | **Desarrollador Backend y Control de Calidad (Híbrido)**       | - Desarrollo de la lógica del **Backend** para el módulo de **Kardex**. <br/>- **Aseguramiento de Calidad (QA):** Tareas de **Testing** funcional y de integración.                                                                                                   | [![GitHub Badge](https://img.shields.io/badge/GitHub-AnthonySGC-100000?style=flat&logo=github&logoColor=white)](https://github.com/AnthonySGC)   |
---

## ✨ Características principales

- Gestión de productos y categorías
- Control de lotes y kardex (historial de movimientos)
- Punto de venta con ticketing y control de stock
- Actualización de inventario en tiempo real con SignalR
- Integración con servicios de autenticación externos
- Servicios y DAOs para separar lógica de negocio y persistencia

---

## 🛠 Tecnologías

- Backend: ASP.NET Core MVC (C#)
- Tiempo real: SignalR
- Persistencia: PostgreSQL (configurable)
- Frontend: Razor Views (\*.cshtml), Bootstrap 5, opcional jQuery
- IDE recomendado: JetBrains Rider (Windows)

---

## 📂 Estructura del proyecto (resumen)

`OrdexIn/`
- `Controllers/` — Controladores MVC y endpoints API
- `Hubs/` — SignalR Hubs (ej.: `InventoryHub`)
- `Models/` — Modelos de dominio y DTOs
- `Services/` — Lógica de negocio, DAOs y servicios externos
- `Views/` — Razor Views (\*.cshtml)
- `wwwroot/` — Assets (css, js, imágenes)
- `docs/` — Documentación (ej.: `ManualUso.md`)

---

## 🔗 Rutas y endpoints relevantes

- `/` — Dashboard / Home
- `/Account/Login` — Inicio de sesión
- `/Product` — Lista y gestión de productos
- `/Product/Details/{id}` — Detalles, lotes y kardex
- `/Management/Users` — Administración de usuarios/roles
- API: `/Api/PointOfSale`, `/Api/Inventory`, `/Api/Product`
- SignalR hub: `/hubs/inventory` (configurable)

---