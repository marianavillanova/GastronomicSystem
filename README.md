🍽️ DESCRIPCIÓN DEL PROYECTO
GastronomicSystem o Gatro-Pouch es una solución para la gestión de un restaurante, diseñada para realizar operaciones como la toma de pedidos, control de mesas, facturación, gestión de artículos y turnos, y generación de reportes.

El sistema está compuesto por:

- API REST desarrollada en ASP.NET Core 8
- Aplicación Frontend desarrollada en Angular
- Base de datos SQL Server, incluida como archivo .bak para restauración

🏗️ ARQUITECTURA DEL SISTEMA
GastronomicSystem/
│
├── API/                     → Backend en ASP.NET Core
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── DTOs/
│   ├── appsettings.json
│   └── RestaurantAPI.csproj
│
├── APP/                     → Frontend en Angular
│   ├── src/
│   ├── angular.json
│   ├── package.json
│   └── tsconfig.json
│
├── DELIVERABLES/
│   └── DB/
│       └── GastronomicSystem.bak   → Backup de la base de datos
│
└── README.md


🗄️ RESTAURACIÓN DE LA DB
1. Abrir SQL Server Management Studio (SSMS)
2. Click derecho en Databases → Restore Database
3. Seleccionar Device
4. Buscar el archivo:
    path: DELIVERABLES/DB/GastronomicSystem.bak
5. Restaurar la base de datos
6. Verificar que las tablas y datos se hayan cargado correctamente


⚙️ CONFIGURACIÓN Y EJECUCIÓN DEL BACKEND (API)
Requisitos:
* .NET 8 SDK
* SQL Server
* Visual Studio 2022 o VS Code

Pasos para ejecutar:
1. Abrir la carpeta API/ en Visual Studio o VS Code
2. Editar appsettings.json y actualizar la cadena de conexión:
   "ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR;Database=GastronomicSystem;Trusted_Connection=True;TrustServerCertificate=True;" }

3. Ejecutar el proyecto con el siguiente comando en la terminal:
  dotnet run
4. La API estará disponible en:
  http://localhost:5151

💻 CCONFIGURACIÓN Y EJECUCIÓN DEL FRONTEND (ANGULAR)
Requisitos:
* Node.js (v18+)
* Angular CLI

Pasos para ejecutar:
1. Abrir la carpeta APP/ en VS Code
2. Para instalar dependencias, ejecutar el siguiente comando en la terminal:
  npm install
3. Ejecutar la aplicación:
   ng serve
4. Abrir en el navegador:
   http://localhost:4200

🔗 CONEXIÓN ENTRE BACKEND Y FRONTEND
En el frontend, la URL de la API se configura en:
  src/environments/environment.ts
Ejemplo:
  export const environment = {
  production: false,
  apiUrl: 'https://localhost:5151/api'
  };


FUNCIONALIDADES PRINCIPALES DEL SISTEMA:
* GESTIÓN DE EMPLEADOS *
  - Inicio y cierre de sesión por turno.
  - Accesibilidad por roles designados.
* GESTIÓN DE MESAS *
  - Apertura y cierre de mesas.
  - Asignación de pedidos.
  - Cambio de estado.
* GESTIÓN DE ARTICULOS (MENÚ DINAMICO) *
  - Listado de productos clasificados.
  - Precio y descripción.
* FACTURACIÓN *
  - Creación y eliminación de facturas.
  - Cierre de cuentas.
* REPORTES *
  - Ventas por dia (arqueo de caja).
  - Articulos mas vendidos.
  - Tipos de clientes.

🧪 TECNOLOGÍAS UTILIZADAS

CAPA           |      TECNOLOGÍA
Backend        |	    ASP.NET Core 8, Entity Framework Core
Frontend	     |      Angular
Base de Datos  |	    SQL Server
Lenguajes	     |      C#, TypeScript, HTML, SCSS
Debug          |      Asistencia con Copilot









