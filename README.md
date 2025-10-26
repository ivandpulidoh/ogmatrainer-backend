# OGMA Trainer Backend

Sistema de gestión integral para gimnasios desarrollado con arquitectura de microservicios en .NET 9.0. OGMA proporciona una plataforma completa para la administración de gimnasios, incluyendo gestión de usuarios, reservas, rutinas de ejercicio, control de capacidad y membresías.

## 🎥 Demo de la Aplicación

[![OGMA Trainer Demo](https://img.youtube.com/vi/tfHN5AVXk74/0.jpg)](https://www.youtube.com/watch?v=tfHN5AVXk74)

**[Ver Demo Completa en YouTube](https://www.youtube.com/watch?v=tfHN5AVXk74)**

Este video muestra una demostración rápida de las principales funcionalidades de la aplicación OGMA Trainer.

## 🏗️ Arquitectura del Sistema

El proyecto está estructurado como una arquitectura de microservicios, donde cada servicio maneja un dominio específico del negocio:

```
ogma_trainer_backend/
├── BookingManagementService/     # Gestión de reservas y citas
├── CapacityControlService/       # Control de aforo y acceso
├── GymManagementService/         # Administración de gimnasios
├── MembershipService/            # Gestión de membresías y pagos
├── NotificationService/          # Sistema de notificaciones
├── RoutineEquipmentService/      # Rutinas, ejercicios y equipamiento
├── UserManagementService/        # Gestión de usuarios y autenticación
└── db/                          # Scripts de base de datos
```
## 📚 Documentación API

Una vez desplegados, los servicios exponen documentación Swagger en:
- UserManagementService: `http://localhost:5161/swagger`
- BookingManagementService: `http://localhost:5080/swagger`
- RoutineEquipmentService: `http://localhost:5067/swagger`

## 🔐 Seguridad

- Autenticación JWT implementada
- Autorización basada en roles
- Validación de entrada en todos los endpoints
- Conexiones seguras a base de datos
- Manejo seguro de contraseñas (hash)

## 🚀 Características Avanzadas

- **IA Integrada**: Generación automática de rutinas usando Gemini AI
- **Códigos QR**: Check-in/check-out automático
- **Notificaciones en Tiempo Real**: Sistema de alertas y recordatorios
- **Control de Aforo**: Monitoreo de capacidad en tiempo real
- **Arquitectura Escalable**: Microservicios independientes
- **Logging Centralizado**: Logs estructurados en todos los servicios

## 📋 Servicios y Funcionalidades

### 1. UserManagementService (Puerto: 5161)
**Responsabilidad**: Gestión de usuarios, autenticación y autorización

**Funcionalidades principales**:
- Registro y autenticación de usuarios
- Gestión de perfiles de usuario
- Sistema de roles (Cliente, Entrenador, Administrador, AdminGimnasio)
- Información personal detallada (objetivos, medidas físicas, experiencia)
- Autenticación JWT

**Endpoints principales**:
- `POST /api/auth/login` - Inicio de sesión
- `GET/POST/PUT /api/users` - CRUD de usuarios
- `GET/POST/PUT /api/personalinformation` - Información personal

### 2. BookingManagementService (Puerto: 5080)
**Responsabilidad**: Gestión de reservas de máquinas, entrenadores y clases

**Funcionalidades principales**:
- Reservas de máquinas de ejercicio
- Reservas de sesiones con entrenadores
- Inscripción a clases grupales
- Reservas vinculadas a rutinas de ejercicio
- Validación de disponibilidad
- Sistema de penalizaciones por no asistencia
- Notificaciones automáticas

**Endpoints principales**:
- `POST /api/bookings/machines` - Crear reserva de máquina
- `POST /api/bookings/trainers` - Crear reserva de entrenador
- `POST /api/bookings/classes/{classId}/register` - Inscribirse a clase
- `GET /api/bookings/user/{userId}/day/{date}` - Reservas del usuario por día
- `POST /api/bookings/routines/book-day` - Reservar día completo de rutina

### 3. CapacityControlService (Puerto: Variable)
**Responsabilidad**: Control de aforo, check-in/check-out y formularios de salud

**Funcionalidades principales**:
- Check-in y check-out con códigos QR
- Control de capacidad máxima del gimnasio
- Formularios de síntomas (COVID-19)
- Historial de asistencia
- Generación de códigos QR dinámicos
- Notificaciones de capacidad

**Endpoints principales**:
- `POST /api/attendance/checkin` - Registro de entrada
- `POST /api/attendance/checkout` - Registro de salida
- `GET /api/capacity/current` - Capacidad actual
- `POST /api/symptoms/submit` - Enviar formulario de síntomas
- `POST /api/qrcode/generate` - Generar código QR

### 4. RoutineEquipmentService (Puerto: 5067)
**Responsabilidad**: Gestión de rutinas, ejercicios, equipamiento y clases

**Funcionalidades principales**:
- Catálogo de ejercicios y máquinas
- Creación y asignación de rutinas personalizadas
- Rutinas generadas por IA (Gemini AI)
- Gestión de espacios deportivos
- Clases en vivo y grabadas
- Vinculación ejercicio-máquina

**Endpoints principales**:
- `GET/POST /api/exercises` - Gestión de ejercicios
- `GET/POST /api/equipment` - Gestión de equipamiento
- `GET/POST/PUT /api/routines` - CRUD de rutinas
- `POST /ai-routines/generate` - Generar rutina con IA
- `GET/POST /api/classes` - Gestión de clases
- `GET/POST /api/espacios` - Gestión de espacios

### 5. GymManagementService (Puerto: Variable)
**Responsabilidad**: Administración de gimnasios y horarios

**Funcionalidades principales**:
- Registro y gestión de gimnasios
- Configuración de horarios de operación
- Asignación de administradores por gimnasio
- Vinculación de entrenadores a gimnasios

**Endpoints principales**:
- `GET/POST /api/gyms` - CRUD de gimnasios
- `GET/POST/PUT /api/gymhours` - Gestión de horarios

### 6. MembershipService (Puerto: Variable)
**Responsabilidad**: Gestión de membresías y procesamiento de pagos

**Funcionalidades principales**:
- Tipos de membresía configurables
- Gestión de membresías activas/inactivas
- Procesamiento de pagos
- Renovación automática
- Historial de transacciones

**Endpoints principales**:
- `GET/POST /api/tiposmembresia` - Tipos de membresía
- `GET/POST /api/membresias` - Gestión de membresías
- `GET/POST /api/pagos` - Procesamiento de pagos

### 7. NotificationService (Puerto: Variable)
**Responsabilidad**: Sistema centralizado de notificaciones

**Funcionalidades principales**:
- Notificaciones push
- Notificaciones por email
- Recordatorios de reservas
- Alertas de capacidad
- Notificaciones personalizadas

**Endpoints principales**:
- `POST /api/notifications` - Crear notificación
- `GET /api/notifications/{userId}` - Obtener notificaciones del usuario

## 🗄️ Base de Datos

El sistema utiliza SQL Server como base de datos principal. La estructura incluye:

**Entidades principales**:
- **Usuarios**: Información básica y roles
- **Gimnasios**: Datos de gimnasios y horarios
- **Equipamiento**: Máquinas y espacios deportivos
- **Reservas**: Reservas de máquinas, entrenadores y espacios
- **Rutinas**: Rutinas de ejercicio y ejercicios
- **Membresías**: Tipos de membresía y pagos
- **Check-ins**: Control de asistencia y aforo

**Scripts disponibles**:
- `db/SCRIPT CREACION OGMA SQLServer.txt` - Creación de tablas
- `db/SCRIPT INSERT OGMA SQLServer.txt` - Datos iniciales

## 🚀 Despliegue con Docker

### Prerrequisitos
- Docker Desktop instalado
- .NET 9.0 SDK (para desarrollo local)
- SQL Server (local o contenedor)

### Construcción de Imágenes

Cada servicio incluye su propio Dockerfile optimizado para .NET 9.0. Para construir las imágenes:

#### 1. BookingManagementService
```bash
cd BookingManagementService
docker build -t ogma/booking-service:latest .
```

#### 2. UserManagementService
```bash
cd UserManagementService
docker build -t ogma/user-service:latest .
```

#### 3. RoutineEquipmentService
```bash
cd RoutineEquipmentService
docker build -t ogma/routine-service:latest .
```

#### 4. CapacityControlService
```bash
cd CapacityControlService
docker build -t ogma/capacity-service:latest .
```

#### 5. GymManagementService
```bash
cd GymManagementService
docker build -t ogma/gym-service:latest .
```

#### 6. MembershipService
```bash
cd MembershipService
docker build -t ogma/membership-service:latest .
```

#### 7. NotificationService
```bash
cd NotificationService
docker build -t ogma/notification-service:latest .
```

### Ejecución de Contenedores

#### Opción 1: Ejecución individual
```bash
# UserManagementService
docker run -d -p 5161:5161 --name ogma-user-service ogma/user-service:latest

# BookingManagementService
docker run -d -p 5080:5080 --name ogma-booking-service ogma/booking-service:latest

# RoutineEquipmentService
docker run -d -p 5067:5067 --name ogma-routine-service ogma/routine-service:latest

# Otros servicios...
```

#### Opción 2: Docker Compose (Recomendado)
Crear un archivo `docker-compose.yml` en la raíz del proyecto:

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  user-service:
    build: ./UserManagementService
    ports:
      - "5161:5161"
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=OgmaDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
    depends_on:
      - sqlserver

  booking-service:
    build: ./BookingManagementService
    ports:
      - "5080:5080"
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=OgmaDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
    depends_on:
      - sqlserver

  routine-service:
    build: ./RoutineEquipmentService
    ports:
      - "5067:5067"
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=OgmaDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
    depends_on:
      - sqlserver

  capacity-service:
    build: ./CapacityControlService
    ports:
      - "5070:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=OgmaDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
    depends_on:
      - sqlserver

  gym-service:
    build: ./GymManagementService
    ports:
      - "5071:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=OgmaDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
    depends_on:
      - sqlserver

  membership-service:
    build: ./MembershipService
    ports:
      - "5072:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=OgmaDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
    depends_on:
      - sqlserver

  notification-service:
    build: ./NotificationService
    ports:
      - "5073:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=OgmaDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
    depends_on:
      - sqlserver

volumes:
  sqlserver_data:
```

Ejecutar con:
```bash
docker-compose up -d
```

### Configuración de Base de Datos

1. **Crear la base de datos**:
```bash
# Conectar a SQL Server y ejecutar los scripts
docker exec -it <sqlserver-container-id> /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd
```

2. **Ejecutar scripts de creación**:
```sql
-- Ejecutar el contenido de db/SCRIPT CREACION OGMA SQLServer.txt
-- Seguido de db/SCRIPT INSERT OGMA SQLServer.txt
```

## 🔧 Configuración

### Variables de Entorno

Cada servicio requiere las siguientes variables de entorno:

```bash
# Conexión a base de datos
ConnectionStrings__DefaultConnection=Server=localhost;Database=OgmaDB;Trusted_Connection=true;

# JWT (para servicios que requieren autenticación)
JWT__SecretKey=your-secret-key-here
JWT__Issuer=OgmaTrainer
JWT__Audience=OgmaTrainerUsers

# APIs externas (RoutineEquipmentService)
GEMINI_API_KEY=your-gemini-api-key

# Configuración de notificaciones
SMTP__Host=smtp.gmail.com
SMTP__Port=587
SMTP__Username=your-email@gmail.com
SMTP__Password=your-app-password
```

### Archivos de Configuración

Cada servicio tiene sus archivos `appsettings.json` y `appsettings.Development.json` para configuración específica.

## 🧪 Testing

Cada servicio incluye archivos `.http` para testing de endpoints:
- `BookingManagementService.http`
- `UserManagementService.http`
- `RoutineEquipmentService.http`
- etc.


## 📞 Soporte

Para soporte técnico o consultas sobre el proyecto, contactar al equipo de desarrollo.

---

**OGMA Trainer Backend** - Sistema integral de gestión para gimnasios modernos.