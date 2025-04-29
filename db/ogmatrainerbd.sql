-- MySQL dump 10.13  Distrib 8.0.38, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: ogmatrainer-db
-- ------------------------------------------------------
-- Server version	9.3.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `alertasusuario`
--

DROP TABLE IF EXISTS `alertasusuario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `alertasusuario` (
  `id_alerta` int NOT NULL AUTO_INCREMENT,
  `id_usuario` int NOT NULL,
  `tipo_alerta` enum('NoAsistenciaReserva','Otro') DEFAULT 'NoAsistenciaReserva',
  `id_reserva_espacio` int DEFAULT NULL,
  `id_reserva_maquina` int DEFAULT NULL,
  `id_reserva_entrenador` int DEFAULT NULL,
  `fecha_alerta` datetime DEFAULT CURRENT_TIMESTAMP,
  `descripcion` text,
  `resuelta` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`id_alerta`),
  KEY `id_usuario` (`id_usuario`),
  KEY `id_reserva_espacio` (`id_reserva_espacio`),
  KEY `id_reserva_maquina` (`id_reserva_maquina`),
  KEY `id_reserva_entrenador` (`id_reserva_entrenador`),
  CONSTRAINT `alertasusuario_ibfk_1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE,
  CONSTRAINT `alertasusuario_ibfk_2` FOREIGN KEY (`id_reserva_espacio`) REFERENCES `reservasespacios` (`id_reserva_espacio`) ON DELETE SET NULL,
  CONSTRAINT `alertasusuario_ibfk_3` FOREIGN KEY (`id_reserva_maquina`) REFERENCES `reservasmaquinas` (`id_reserva_maquina`) ON DELETE SET NULL,
  CONSTRAINT `alertasusuario_ibfk_4` FOREIGN KEY (`id_reserva_entrenador`) REFERENCES `reservasentrenador` (`id_reserva_entrenador`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `alertasusuario`
--

LOCK TABLES `alertasusuario` WRITE;
/*!40000 ALTER TABLE `alertasusuario` DISABLE KEYS */;
INSERT INTO `alertasusuario` VALUES (1,1,'NoAsistenciaReserva',5,NULL,NULL,'2025-04-23 15:36:25','Usuario no asistió a la reserva del Espacio ID 5.',0),(2,2,'Otro',NULL,NULL,NULL,'2025-04-23 15:36:25','Usuario dejó equipo desordenado en zona de peso libre.',0),(3,6,'NoAsistenciaReserva',NULL,NULL,NULL,'2025-04-23 15:36:25','Falta registrada manualmente por entrenador para sesión no reservada.',1),(4,9,'Otro',NULL,NULL,NULL,'2025-04-23 15:36:25','Recordatorio de pago de cuota pendiente.',0),(5,1,'NoAsistenciaReserva',NULL,NULL,5,'2025-04-23 15:36:25','Cliente no asistió a la sesión con Entrenador ID 3.',0);
/*!40000 ALTER TABLE `alertasusuario` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `checkins`
--

DROP TABLE IF EXISTS `checkins`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `checkins` (
  `id_checkin` int NOT NULL AUTO_INCREMENT,
  `id_usuario` int NOT NULL,
  `id_gimnasio` int NOT NULL,
  `hora_entrada` datetime DEFAULT CURRENT_TIMESTAMP,
  `hora_salida` datetime DEFAULT NULL,
  PRIMARY KEY (`id_checkin`),
  KEY `id_usuario` (`id_usuario`),
  KEY `id_gimnasio` (`id_gimnasio`),
  CONSTRAINT `checkins_ibfk_1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE,
  CONSTRAINT `checkins_ibfk_2` FOREIGN KEY (`id_gimnasio`) REFERENCES `gimnasios` (`id_gimnasio`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `checkins`
--

LOCK TABLES `checkins` WRITE;
/*!40000 ALTER TABLE `checkins` DISABLE KEYS */;
INSERT INTO `checkins` VALUES (1,1,1,'2025-04-23 13:06:05','2025-04-23 14:21:05'),(2,2,2,'2025-04-23 14:36:05',NULL),(3,6,1,'2025-04-23 10:36:05','2025-04-23 11:51:05'),(4,9,3,'2025-04-22 11:36:05','2025-04-22 13:06:05'),(5,10,5,'2025-04-21 09:36:05','2025-04-21 10:36:05');
/*!40000 ALTER TABLE `checkins` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `clases`
--

DROP TABLE IF EXISTS `clases`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clases` (
  `id_clase` int NOT NULL AUTO_INCREMENT,
  `id_gimnasio` int NOT NULL,
  `id_entrenador` int DEFAULT NULL,
  `nombre_clase` varchar(150) NOT NULL,
  `descripcion` text,
  `tipo` enum('EnVivo','Grabada') NOT NULL,
  `url_clase` varchar(255) DEFAULT NULL COMMENT 'URL for live stream or recorded video',
  `fecha_hora_inicio` datetime DEFAULT NULL COMMENT 'Required for live classes',
  `duracion_minutos` int DEFAULT NULL,
  `capacidad_maxima` int DEFAULT NULL COMMENT 'For live classes with limited spots',
  `activa` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`id_clase`),
  KEY `id_gimnasio` (`id_gimnasio`),
  KEY `id_entrenador` (`id_entrenador`),
  CONSTRAINT `clases_ibfk_1` FOREIGN KEY (`id_gimnasio`) REFERENCES `gimnasios` (`id_gimnasio`) ON DELETE CASCADE,
  CONSTRAINT `clases_ibfk_2` FOREIGN KEY (`id_entrenador`) REFERENCES `usuarios` (`id_usuario`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `clases`
--

LOCK TABLES `clases` WRITE;
/*!40000 ALTER TABLE `clases` DISABLE KEYS */;
INSERT INTO `clases` VALUES (1,1,3,'Yoga Vinyasa Flow','Clase dinámica de yoga para todos los niveles.','EnVivo','https://meet.example.com/yoga1','2025-04-25 15:58:33',60,15,1),(2,3,7,'HIIT Intenso','Entrenamiento interválico de alta intensidad.','EnVivo','https://meet.example.com/hiit1','2025-04-26 15:46:33',45,20,1),(3,5,NULL,'Aqua Fitness Básico','Ejercicios de bajo impacto en la piscina.','EnVivo',NULL,'2025-04-24 15:50:33',50,18,1),(4,1,3,'Introducción al Levantamiento Olímpico','Taller práctico sobre Snatch y Clean & Jerk.','Grabada','https://video.example.com/olyintro',NULL,90,NULL,1),(5,3,7,'Spinning Challenge','Clase de ciclismo indoor con música motivadora.','EnVivo','https://meet.example.com/spin1','2025-04-27 15:57:33',55,25,0);
/*!40000 ALTER TABLE `clases` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ejercicios`
--

DROP TABLE IF EXISTS `ejercicios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ejercicios` (
  `id_ejercicio` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(150) NOT NULL,
  `descripcion` text,
  `musculo_objetivo` varchar(100) DEFAULT NULL,
  `url_video_demostracion` varchar(255) DEFAULT NULL,
  `id_creador` int DEFAULT NULL,
  PRIMARY KEY (`id_ejercicio`),
  UNIQUE KEY `nombre` (`nombre`),
  KEY `id_creador` (`id_creador`),
  CONSTRAINT `ejercicios_ibfk_1` FOREIGN KEY (`id_creador`) REFERENCES `usuarios` (`id_usuario`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ejercicios`
--

LOCK TABLES `ejercicios` WRITE;
/*!40000 ALTER TABLE `ejercicios` DISABLE KEYS */;
INSERT INTO `ejercicios` VALUES (1,'Press de Banca con Barra','Ejercicio compuesto para pectoral, hombros y tríceps.','Pectoral, Hombros, Tríceps','https://video.example.com/pressbanca',3),(2,'Sentadilla Trasera','Ejercicio fundamental para piernas y glúteos.','Cuádriceps, Glúteos, Isquiotibiales','https://video.example.com/sentadilla',7),(3,'Remo con Barra T','Ejercicio de tracción para la espalda alta y bíceps.','Dorsales, Romboides, Bíceps','https://video.example.com/remobarra',3),(4,'Plancha Abdominal','Ejercicio isométrico para el core.','Abdominales, Lumbares, Core','https://video.example.com/plancha',NULL),(5,'Zancadas con Mancuernas','Ejercicio unilateral para piernas y glúteos.','Cuádriceps, Glúteos','https://video.example.com/zancadas',7);
/*!40000 ALTER TABLE `ejercicios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `entrenadorgimnasios`
--

DROP TABLE IF EXISTS `entrenadorgimnasios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `entrenadorgimnasios` (
  `id_usuario` int NOT NULL,
  `id_gimnasio` int NOT NULL,
  PRIMARY KEY (`id_usuario`,`id_gimnasio`),
  KEY `id_gimnasio` (`id_gimnasio`),
  CONSTRAINT `entrenadorgimnasios_ibfk_1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE,
  CONSTRAINT `entrenadorgimnasios_ibfk_2` FOREIGN KEY (`id_gimnasio`) REFERENCES `gimnasios` (`id_gimnasio`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `entrenadorgimnasios`
--

LOCK TABLES `entrenadorgimnasios` WRITE;
/*!40000 ALTER TABLE `entrenadorgimnasios` DISABLE KEYS */;
INSERT INTO `entrenadorgimnasios` VALUES (2,1),(3,1),(7,2),(3,3),(7,4),(3,5);
/*!40000 ALTER TABLE `entrenadorgimnasios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `espaciosdeportivos`
--

DROP TABLE IF EXISTS `espaciosdeportivos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `espaciosdeportivos` (
  `id_espacio` int NOT NULL AUTO_INCREMENT,
  `id_gimnasio` int NOT NULL,
  `nombre_espacio` varchar(100) NOT NULL,
  `descripcion` text,
  `capacidad` int DEFAULT '1',
  `reservable` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`id_espacio`),
  UNIQUE KEY `uk_gimnasio_espacio_nombre` (`id_gimnasio`,`nombre_espacio`),
  CONSTRAINT `espaciosdeportivos_ibfk_1` FOREIGN KEY (`id_gimnasio`) REFERENCES `gimnasios` (`id_gimnasio`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `espaciosdeportivos`
--

LOCK TABLES `espaciosdeportivos` WRITE;
/*!40000 ALTER TABLE `espaciosdeportivos` DISABLE KEYS */;
INSERT INTO `espaciosdeportivos` VALUES (1,1,'Sala de Pesas Principal','Zona con máquinas de fuerza y pesos libres',30,0),(2,1,'Estudio de Yoga','Espacio tranquilo para clases de yoga y meditación',15,1),(3,2,'Zona Cardio','Área con cintas, elípticas y bicicletas',25,0),(4,3,'Piscina Semi-Olímpica','Piscina climatizada para natación y clases',20,1),(5,5,'Sala Funcional','Espacio para entrenamiento funcional y HIIT',18,1);
/*!40000 ALTER TABLE `espaciosdeportivos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `formulariossintomas`
--

DROP TABLE IF EXISTS `formulariossintomas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `formulariossintomas` (
  `id_formulario` int NOT NULL AUTO_INCREMENT,
  `id_checkin` int NOT NULL,
  `id_usuario` int NOT NULL,
  `fecha_envio` datetime DEFAULT CURRENT_TIMESTAMP,
  `tiene_sintomas` tinyint(1) NOT NULL,
  `tuvo_contacto_reciente` tinyint(1) NOT NULL,
  `resultado_evaluacion` enum('Aprobado','Rechazado') NOT NULL,
  PRIMARY KEY (`id_formulario`),
  UNIQUE KEY `id_checkin` (`id_checkin`),
  KEY `id_usuario` (`id_usuario`),
  CONSTRAINT `formulariossintomas_ibfk_1` FOREIGN KEY (`id_checkin`) REFERENCES `checkins` (`id_checkin`) ON DELETE CASCADE,
  CONSTRAINT `formulariossintomas_ibfk_2` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `formulariossintomas`
--

LOCK TABLES `formulariossintomas` WRITE;
/*!40000 ALTER TABLE `formulariossintomas` DISABLE KEYS */;
INSERT INTO `formulariossintomas` VALUES (1,1,1,'2025-04-23 13:05:10',0,0,'Aprobado'),(2,2,2,'2025-04-23 14:35:10',0,0,'Aprobado'),(3,3,6,'2025-04-23 10:35:10',0,0,'Aprobado'),(4,4,9,'2025-04-22 11:35:10',0,0,'Aprobado'),(5,5,10,'2025-04-21 09:35:10',1,0,'Rechazado');
/*!40000 ALTER TABLE `formulariossintomas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `gimnasioadministradores`
--

DROP TABLE IF EXISTS `gimnasioadministradores`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `gimnasioadministradores` (
  `id_gimnasio` int NOT NULL,
  `id_usuario` int NOT NULL,
  PRIMARY KEY (`id_gimnasio`,`id_usuario`),
  KEY `id_usuario` (`id_usuario`),
  CONSTRAINT `gimnasioadministradores_ibfk_1` FOREIGN KEY (`id_gimnasio`) REFERENCES `gimnasios` (`id_gimnasio`) ON DELETE CASCADE,
  CONSTRAINT `gimnasioadministradores_ibfk_2` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `gimnasioadministradores`
--

LOCK TABLES `gimnasioadministradores` WRITE;
/*!40000 ALTER TABLE `gimnasioadministradores` DISABLE KEYS */;
INSERT INTO `gimnasioadministradores` VALUES (1,4),(3,4),(4,5),(2,8),(5,8);
/*!40000 ALTER TABLE `gimnasioadministradores` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `gimnasios`
--

DROP TABLE IF EXISTS `gimnasios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `gimnasios` (
  `id_gimnasio` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(150) NOT NULL,
  `direccion` text NOT NULL,
  `capacidad_maxima` int NOT NULL DEFAULT '100',
  `activo` tinyint(1) DEFAULT '1',
  `codigo_qr_entrada` blob,
  `codigo_qr_salida` blob,
  PRIMARY KEY (`id_gimnasio`),
  UNIQUE KEY `nombre` (`nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `gimnasios`
--

LOCK TABLES `gimnasios` WRITE;
/*!40000 ALTER TABLE `gimnasios` DISABLE KEYS */;
INSERT INTO `gimnasios` VALUES (1,'Gimnasio Central Fit','Calle Falsa 123, Ciudad Capital',150,1,NULL,NULL),(2,'Iron Body Gym','Avenida Siempre Viva 742, Pueblo Primavera',80,1,NULL,NULL),(3,'Zenith Fitness Club','Boulevard del Sol 45, Metrópolis Oeste',200,1,NULL,NULL),(4,'Spartan Strength Center','Plaza Mayor 1, Villa Roca',120,0,NULL,NULL),(5,'Aqua Gym & Spa','Ruta Acuática 88, Costa Serena',90,1,NULL,NULL);
/*!40000 ALTER TABLE `gimnasios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `horariosgimnasio`
--

DROP TABLE IF EXISTS `horariosgimnasio`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `horariosgimnasio` (
  `id_horario_gimnasio` int NOT NULL AUTO_INCREMENT,
  `id_gimnasio` int NOT NULL,
  `dia_semana` enum('Lunes','Martes','Miercoles','Jueves','Viernes','Sabado','Domingo') NOT NULL,
  `hora_apertura` time NOT NULL,
  `hora_cierre` time NOT NULL,
  PRIMARY KEY (`id_horario_gimnasio`),
  UNIQUE KEY `uk_gimnasio_dia` (`id_gimnasio`,`dia_semana`),
  CONSTRAINT `horariosgimnasio_ibfk_1` FOREIGN KEY (`id_gimnasio`) REFERENCES `gimnasios` (`id_gimnasio`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `horariosgimnasio`
--

LOCK TABLES `horariosgimnasio` WRITE;
/*!40000 ALTER TABLE `horariosgimnasio` DISABLE KEYS */;
INSERT INTO `horariosgimnasio` VALUES (1,1,'Lunes','06:00:00','22:00:00'),(2,1,'Martes','06:00:00','22:00:00'),(3,2,'Lunes','07:00:00','21:00:00'),(4,3,'Sabado','08:00:00','20:00:00'),(5,5,'Domingo','09:00:00','15:00:00'),(6,1,'Viernes','08:00:00','21:30:00'),(7,1,'Jueves','08:00:00','21:30:00');
/*!40000 ALTER TABLE `horariosgimnasio` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `inscripcionesclases`
--

DROP TABLE IF EXISTS `inscripcionesclases`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inscripcionesclases` (
  `id_inscripcion` int NOT NULL AUTO_INCREMENT,
  `id_usuario` int NOT NULL,
  `id_clase` int NOT NULL,
  `fecha_inscripcion` datetime DEFAULT CURRENT_TIMESTAMP,
  `asistio` tinyint(1) DEFAULT NULL COMMENT 'Track attendance for live classes',
  PRIMARY KEY (`id_inscripcion`),
  UNIQUE KEY `uk_usuario_clase` (`id_usuario`,`id_clase`),
  KEY `id_clase` (`id_clase`),
  CONSTRAINT `inscripcionesclases_ibfk_1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE,
  CONSTRAINT `inscripcionesclases_ibfk_2` FOREIGN KEY (`id_clase`) REFERENCES `clases` (`id_clase`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inscripcionesclases`
--

LOCK TABLES `inscripcionesclases` WRITE;
/*!40000 ALTER TABLE `inscripcionesclases` DISABLE KEYS */;
INSERT INTO `inscripcionesclases` VALUES (1,1,1,'2025-04-22 15:39:35',NULL),(2,2,2,'2025-04-22 15:39:35',NULL),(3,6,3,'2025-04-23 15:39:35',NULL),(4,9,4,'2025-04-18 15:39:35',NULL),(5,1,2,'2025-04-21 15:39:35',NULL);
/*!40000 ALTER TABLE `inscripcionesclases` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mantenimientosequipos`
--

DROP TABLE IF EXISTS `mantenimientosequipos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mantenimientosequipos` (
  `id_mantenimiento` int NOT NULL AUTO_INCREMENT,
  `id_maquina` int NOT NULL,
  `id_usuario_reporta` int DEFAULT NULL,
  `id_usuario_atiende` int DEFAULT NULL,
  `fecha_reporte` datetime DEFAULT CURRENT_TIMESTAMP,
  `descripcion_falla` text NOT NULL,
  `fecha_inicio_mantenimiento` datetime DEFAULT NULL,
  `fecha_fin_mantenimiento` datetime DEFAULT NULL,
  `estado_mantenimiento` enum('Reportada','EnProgreso','Resuelta','Cancelada') DEFAULT 'Reportada',
  `notas_mantenimiento` text,
  PRIMARY KEY (`id_mantenimiento`),
  KEY `id_maquina` (`id_maquina`),
  KEY `id_usuario_reporta` (`id_usuario_reporta`),
  KEY `id_usuario_atiende` (`id_usuario_atiende`),
  CONSTRAINT `mantenimientosequipos_ibfk_1` FOREIGN KEY (`id_maquina`) REFERENCES `maquinasejercicio` (`id_maquina`) ON DELETE CASCADE,
  CONSTRAINT `mantenimientosequipos_ibfk_2` FOREIGN KEY (`id_usuario_reporta`) REFERENCES `usuarios` (`id_usuario`) ON DELETE SET NULL,
  CONSTRAINT `mantenimientosequipos_ibfk_3` FOREIGN KEY (`id_usuario_atiende`) REFERENCES `usuarios` (`id_usuario`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mantenimientosequipos`
--

LOCK TABLES `mantenimientosequipos` WRITE;
/*!40000 ALTER TABLE `mantenimientosequipos` DISABLE KEYS */;
INSERT INTO `mantenimientosequipos` VALUES (1,3,1,4,'2025-04-21 15:35:22','La cinta hace un ruido extraño al superar los 10 km/h','2025-04-22 15:35:22',NULL,'EnProgreso','Revisando motor y banda.'),(2,5,2,8,'2025-04-18 15:35:22','La pantalla no enciende y la cadena parece suelta.',NULL,NULL,'Reportada','Pendiente de revisión por técnico externo.'),(3,1,6,4,'2025-04-13 15:35:22','El ajuste del asiento está duro.','2025-04-14 15:35:22',NULL,'Resuelta','Se lubricó el mecanismo. Funciona correctamente.'),(4,4,9,8,'2025-04-22 15:35:22','Uno de los pedales tiene juego.','2025-04-23 15:35:22',NULL,'EnProgreso','Ajustando tornillería del pedal derecho.'),(5,3,1,4,'2025-03-24 15:35:22','Botón de parada de emergencia no funciona','2025-03-25 15:35:22',NULL,'Cancelada','Usuario reportó por error, sí funciona.');
/*!40000 ALTER TABLE `mantenimientosequipos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `maquinasejercicio`
--

DROP TABLE IF EXISTS `maquinasejercicio`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `maquinasejercicio` (
  `id_maquina` int NOT NULL AUTO_INCREMENT,
  `id_espacio` int NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `tipo_maquina` varchar(100) DEFAULT NULL,
  `descripcion` text,
  `fecha_adquisicion` date DEFAULT NULL,
  `estado` enum('Disponible','EnMantenimiento','Averiada','Desactivada') NOT NULL DEFAULT 'Disponible',
  `reservable` tinyint(1) DEFAULT '1',
  `codigo_qr` blob,
  PRIMARY KEY (`id_maquina`),
  KEY `id_espacio` (`id_espacio`),
  CONSTRAINT `maquinasejercicio_ibfk_1` FOREIGN KEY (`id_espacio`) REFERENCES `espaciosdeportivos` (`id_espacio`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `maquinasejercicio`
--

LOCK TABLES `maquinasejercicio` WRITE;
/*!40000 ALTER TABLE `maquinasejercicio` DISABLE KEYS */;
INSERT INTO `maquinasejercicio` VALUES (1,1,'Prensa de Piernas Hoist','Fuerza','Máquina para ejercitar cuádriceps y glúteos','2022-01-15','Disponible',1,NULL),(2,1,'Banco Press Banca Olímpico','Fuerza','Banco plano para press de banca con barra','2022-01-15','Disponible',0,NULL),(3,3,'Cinta de Correr LifeFitness 1','Cardio','Cinta de correr profesional con inclinación','2021-11-20','EnMantenimiento',1,NULL),(4,3,'Elíptica Precor EFX 885','Cardio','Máquina elíptica de bajo impacto','2023-03-10','Disponible',1,NULL),(5,5,'Remadora Concept2 Model D','Cardio/Funcional','Remo indoor para entrenamiento completo','2022-08-01','Averiada',0,NULL);
/*!40000 ALTER TABLE `maquinasejercicio` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `membresias`
--

DROP TABLE IF EXISTS `membresias`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `membresias` (
  `id_membresia` int NOT NULL AUTO_INCREMENT,
  `id_usuario` int NOT NULL,
  `id_tipo_membresia` int NOT NULL,
  `id_gimnasio_principal` int DEFAULT NULL,
  `fecha_inicio` date NOT NULL,
  `fecha_fin` date NOT NULL,
  `estado` enum('Activa','Inactiva','Expirada','PendientePago','Cancelada') DEFAULT 'PendientePago',
  `fecha_compra` datetime DEFAULT CURRENT_TIMESTAMP,
  `auto_renovar` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`id_membresia`),
  KEY `id_tipo_membresia` (`id_tipo_membresia`),
  KEY `id_gimnasio_principal` (`id_gimnasio_principal`),
  KEY `idx_usuario_fechas` (`id_usuario`,`fecha_inicio`,`fecha_fin`),
  CONSTRAINT `membresias_ibfk_1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE,
  CONSTRAINT `membresias_ibfk_2` FOREIGN KEY (`id_tipo_membresia`) REFERENCES `tiposmembresia` (`id_tipo_membresia`),
  CONSTRAINT `membresias_ibfk_3` FOREIGN KEY (`id_gimnasio_principal`) REFERENCES `gimnasios` (`id_gimnasio`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `membresias`
--

LOCK TABLES `membresias` WRITE;
/*!40000 ALTER TABLE `membresias` DISABLE KEYS */;
INSERT INTO `membresias` VALUES (1,1,1,1,'2025-04-08','2025-05-08','Activa','2025-04-23 15:39:42',0),(2,2,2,NULL,'2024-11-23','2024-12-23','Expirada','2025-04-23 15:39:42',0),(3,6,4,1,'2025-01-23','2025-07-23','Activa','2025-04-23 15:39:42',1),(4,9,3,3,'2024-04-24','2025-04-24','Activa','2025-04-23 15:39:42',0),(5,10,1,5,'2025-02-23','2025-03-23','Expirada','2025-04-23 15:39:42',0);
/*!40000 ALTER TABLE `membresias` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `notificaciones`
--

DROP TABLE IF EXISTS `notificaciones`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `notificaciones` (
  `id` char(36) NOT NULL,
  `id_usuario` int NOT NULL,
  `tipo` varchar(100) NOT NULL,
  `fecha` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `nombre` varchar(255) NOT NULL,
  `descripcion` text,
  PRIMARY KEY (`id`),
  KEY `id_usuario` (`id_usuario`),
  CONSTRAINT `notificaciones_ibfk_1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notificaciones`
--

LOCK TABLES `notificaciones` WRITE;
/*!40000 ALTER TABLE `notificaciones` DISABLE KEYS */;
INSERT INTO `notificaciones` VALUES ('2fdbda14-230a-11f0-bb66-505a65eeb4f4',1,'Recordatorio de reserva','2025-04-26 20:51:50','Recordatorio de tu reserva','Recuerda que tienes una reserva hoy en el gimnasio.'),('2fdc7b42-230a-11f0-bb66-505a65eeb4f4',2,'Aviso de vencimiento de membresía','2025-04-26 20:51:50','Tu membresía está por vencer','Tu membresía vence en 3 días. Renueva para seguir entrenando.'),('2fdc88fa-230a-11f0-bb66-505a65eeb4f4',3,'Actualizaciones de progreso','2025-04-26 20:51:50','Actualización de tu progreso','¡Felicidades! Has completado el 70% de tu objetivo mensual.'),('2fdc8bf5-230a-11f0-bb66-505a65eeb4f4',4,'Alerta por no iniciar entrenamiento','2025-04-26 20:51:50','¡Tu máquina te espera!','Has reservado una máquina pero aún no inicias tu entrenamiento.'),('2fdc8dd2-230a-11f0-bb66-505a65eeb4f4',5,'Alerta por no haber asistido a su reserva','2025-04-26 20:51:50','No asististe a tu reserva','Notamos que no asististe a tu reserva. Recuerda cancelar si no puedes venir.');
/*!40000 ALTER TABLE `notificaciones` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pagos`
--

DROP TABLE IF EXISTS `pagos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pagos` (
  `id_pago` int NOT NULL AUTO_INCREMENT,
  `id_usuario` int NOT NULL,
  `id_membresia` int DEFAULT NULL,
  `monto` decimal(10,2) NOT NULL,
  `moneda` varchar(3) DEFAULT 'USD',
  `fecha_pago` datetime DEFAULT CURRENT_TIMESTAMP,
  `metodo_pago` varchar(50) DEFAULT NULL,
  `id_transaccion_externa` varchar(100) DEFAULT NULL,
  `estado_pago` enum('Pendiente','Completado','Fallido','Reembolsado') DEFAULT 'Pendiente',
  `descripcion` text,
  PRIMARY KEY (`id_pago`),
  UNIQUE KEY `id_transaccion_externa` (`id_transaccion_externa`),
  KEY `id_usuario` (`id_usuario`),
  KEY `id_membresia` (`id_membresia`),
  CONSTRAINT `pagos_ibfk_1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE,
  CONSTRAINT `pagos_ibfk_2` FOREIGN KEY (`id_membresia`) REFERENCES `membresias` (`id_membresia`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pagos`
--

LOCK TABLES `pagos` WRITE;
/*!40000 ALTER TABLE `pagos` DISABLE KEYS */;
INSERT INTO `pagos` VALUES (1,1,1,30.00,'EUR','2025-04-23 15:39:48','TarjetaCredito','txn_abc123','Completado','Pago Membresía Mensual Básico'),(2,6,3,150.00,'EUR','2025-04-23 15:39:48','PayPal','pp_def456','Completado','Pago Plan Estudiante Semestral'),(3,9,4,550.00,'EUR','2025-04-23 15:39:48','Stripe','stripe_ghi789','Completado','Pago Membresía Anual Oro'),(4,2,NULL,50.00,'EUR','2025-04-23 15:39:48','TarjetaCredito','txn_jkl012','Fallido','Intento Renovación Mensual Premium'),(5,1,NULL,15.00,'EUR','2025-04-23 15:39:48','Bizum','biz_mno345','Completado','Pago Sesión Entrenador Personal');
/*!40000 ALTER TABLE `pagos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `personalinformation`
--

DROP TABLE IF EXISTS `personalinformation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `personalinformation` (
  `id_usuario` int NOT NULL,
  `altura_cm` decimal(5,1) DEFAULT NULL COMMENT 'Altura del usuario en centímetros (e.g., 175.5)',
  `peso_inicial_kg` decimal(5,2) DEFAULT NULL COMMENT 'Peso del usuario al registrarse o iniciar un plan, en kg (e.g., 80.50)',
  `peso_actual_kg` decimal(5,2) DEFAULT NULL COMMENT 'Peso más reciente registrado por el usuario, en kg',
  `peso_objetivo_kg` decimal(5,2) DEFAULT NULL COMMENT 'Peso que el usuario desea alcanzar, en kg',
  `objetivo_principal` text COMMENT 'Descripción del objetivo principal del usuario (ej: perder peso, ganar músculo, mejorar resistencia)',
  `experiencia_entrenamiento` enum('Principiante','Intermedio','Avanzado','Ninguna') DEFAULT NULL COMMENT 'Nivel de experiencia previa en entrenamiento',
  `nivel_actividad_diaria` enum('Sedentario','Ligero','Moderado','Activo','MuyActivo') DEFAULT NULL COMMENT 'Nivel general de actividad física fuera del gimnasio (trabajo, hobbies)',
  `condiciones_medicas` text COMMENT 'Descripción de condiciones médicas relevantes (alergias, lesiones, enfermedades crónicas)',
  `disponibilidad_entrenamiento` text COMMENT 'Texto libre describiendo días/horas preferidos o disponibles para entrenar',
  `preferencia_lugar_entrenamiento` enum('Casa','AireLibre','Gimnasio','Mixto','Indiferente') DEFAULT NULL COMMENT 'Lugar preferido por el usuario para realizar sus entrenamientos',
  `fecha_creacion` datetime DEFAULT CURRENT_TIMESTAMP,
  `fecha_ultima_actualizacion` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_usuario`),
  CONSTRAINT `personalinformation_ibfk_1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `personalinformation`
--

LOCK TABLES `personalinformation` WRITE;
/*!40000 ALTER TABLE `personalinformation` DISABLE KEYS */;
INSERT INTO `personalinformation` VALUES (11,178.5,85.00,82.30,78.00,'Perder grasa corporal y ganar algo de músculo.','Intermedio','Ligero','Ninguna condición médica relevante.','Lunes, Miércoles y Viernes por la tarde (18:00 - 20:00). Sábados por la mañana.','Gimnasio','2025-04-24 21:20:20','2025-04-24 21:20:20');
/*!40000 ALTER TABLE `personalinformation` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `registrosentrenamiento`
--

DROP TABLE IF EXISTS `registrosentrenamiento`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `registrosentrenamiento` (
  `id_registro` int NOT NULL AUTO_INCREMENT,
  `id_usuario` int NOT NULL,
  `id_ejercicio` int NOT NULL,
  `id_rutina_ejercicio` int DEFAULT NULL,
  `fecha_hora` datetime DEFAULT CURRENT_TIMESTAMP,
  `series_completadas` int DEFAULT NULL,
  `repeticiones_completadas` varchar(50) DEFAULT NULL,
  `peso_utilizado` decimal(10,2) DEFAULT NULL,
  `duracion_minutos` int DEFAULT NULL,
  `notas_usuario` text,
  PRIMARY KEY (`id_registro`),
  KEY `id_usuario` (`id_usuario`),
  KEY `id_ejercicio` (`id_ejercicio`),
  KEY `id_rutina_ejercicio` (`id_rutina_ejercicio`),
  CONSTRAINT `registrosentrenamiento_ibfk_1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE,
  CONSTRAINT `registrosentrenamiento_ibfk_2` FOREIGN KEY (`id_ejercicio`) REFERENCES `ejercicios` (`id_ejercicio`) ON DELETE CASCADE,
  CONSTRAINT `registrosentrenamiento_ibfk_3` FOREIGN KEY (`id_rutina_ejercicio`) REFERENCES `rutinaejercicios` (`id_rutina_ejercicio`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `registrosentrenamiento`
--

LOCK TABLES `registrosentrenamiento` WRITE;
/*!40000 ALTER TABLE `registrosentrenamiento` DISABLE KEYS */;
/*!40000 ALTER TABLE `registrosentrenamiento` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `reservasentrenador`
--

DROP TABLE IF EXISTS `reservasentrenador`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reservasentrenador` (
  `id_reserva_entrenador` int NOT NULL AUTO_INCREMENT,
  `id_cliente` int NOT NULL,
  `id_entrenador` int NOT NULL,
  `id_espacio` int DEFAULT NULL,
  `fecha_hora_inicio` datetime NOT NULL,
  `fecha_hora_fin` datetime NOT NULL,
  `fecha_creacion` datetime DEFAULT CURRENT_TIMESTAMP,
  `estado` enum('Confirmada','Cancelada','Completada','NoShowCliente','NoShowEntrenador') DEFAULT 'Confirmada',
  `asistio_cliente` tinyint(1) DEFAULT NULL,
  `asistio_entrenador` tinyint(1) DEFAULT NULL,
  `notas` text,
  PRIMARY KEY (`id_reserva_entrenador`),
  KEY `id_espacio` (`id_espacio`),
  KEY `idx_entrenador_tiempo` (`id_entrenador`,`fecha_hora_inicio`,`fecha_hora_fin`),
  KEY `idx_cliente_tiempo` (`id_cliente`,`fecha_hora_inicio`),
  CONSTRAINT `reservasentrenador_ibfk_1` FOREIGN KEY (`id_cliente`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE,
  CONSTRAINT `reservasentrenador_ibfk_2` FOREIGN KEY (`id_entrenador`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE,
  CONSTRAINT `reservasentrenador_ibfk_3` FOREIGN KEY (`id_espacio`) REFERENCES `espaciosdeportivos` (`id_espacio`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reservasentrenador`
--

LOCK TABLES `reservasentrenador` WRITE;
/*!40000 ALTER TABLE `reservasentrenador` DISABLE KEYS */;
INSERT INTO `reservasentrenador` VALUES (1,1,3,2,'2025-04-25 15:44:59','2025-04-25 15:45:59','2025-04-23 15:35:59','Confirmada',NULL,NULL,NULL),(2,2,7,5,'2025-04-26 15:52:59','2025-04-26 15:53:59','2025-04-23 15:35:59','Confirmada',NULL,NULL,NULL),(3,6,3,NULL,'2025-04-27 15:46:59','2025-04-27 15:47:59','2025-04-23 15:35:59','Confirmada',NULL,NULL,NULL),(4,9,7,1,'2025-04-22 15:45:59','2025-04-22 15:46:59','2025-04-23 15:35:59','Completada',1,1,NULL),(5,1,3,2,'2025-04-18 15:49:59','2025-04-18 15:50:59','2025-04-23 15:35:59','NoShowCliente',0,1,NULL);
/*!40000 ALTER TABLE `reservasentrenador` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `reservasespacios`
--

DROP TABLE IF EXISTS `reservasespacios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reservasespacios` (
  `id_reserva_espacio` int NOT NULL AUTO_INCREMENT,
  `id_usuario` int NOT NULL,
  `id_espacio` int NOT NULL,
  `fecha_hora_inicio` datetime NOT NULL,
  `fecha_hora_fin` datetime NOT NULL,
  `fecha_creacion` datetime DEFAULT CURRENT_TIMESTAMP,
  `estado` enum('Confirmada','Cancelada','Completada','NoShow') DEFAULT 'Confirmada',
  `asistio` tinyint(1) DEFAULT NULL COMMENT 'Explicit flag for attendance - RF14',
  `notificacion_fin_enviada` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`id_reserva_espacio`),
  KEY `idx_espacio_tiempo` (`id_espacio`,`fecha_hora_inicio`,`fecha_hora_fin`),
  KEY `idx_usuario_tiempo` (`id_usuario`,`fecha_hora_inicio`),
  CONSTRAINT `reservasespacios_ibfk_1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE,
  CONSTRAINT `reservasespacios_ibfk_2` FOREIGN KEY (`id_espacio`) REFERENCES `espaciosdeportivos` (`id_espacio`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reservasespacios`
--

LOCK TABLES `reservasespacios` WRITE;
/*!40000 ALTER TABLE `reservasespacios` DISABLE KEYS */;
INSERT INTO `reservasespacios` VALUES (1,1,2,'2025-04-24 15:45:28','2025-04-24 15:46:28','2025-04-23 15:35:28','Confirmada',NULL,0),(2,2,4,'2025-04-25 15:43:28','2025-04-25 15:44:28','2025-04-23 15:35:28','Confirmada',NULL,0),(3,6,5,'2025-04-24 15:53:28','2025-04-24 15:54:28','2025-04-23 15:35:28','Confirmada',NULL,0),(4,9,2,'2025-04-22 15:50:28','2025-04-22 15:51:28','2025-04-23 15:35:28','Completada',1,0),(5,1,5,'2025-04-21 15:44:28','2025-04-21 15:45:28','2025-04-23 15:35:28','NoShow',0,0);
/*!40000 ALTER TABLE `reservasespacios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `reservasmaquinas`
--

DROP TABLE IF EXISTS `reservasmaquinas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reservasmaquinas` (
  `id_reserva_maquina` int NOT NULL AUTO_INCREMENT,
  `id_usuario` int NOT NULL,
  `id_maquina` int NOT NULL,
  `fecha_hora_inicio` datetime NOT NULL,
  `fecha_hora_fin` datetime NOT NULL,
  `fecha_creacion` datetime DEFAULT CURRENT_TIMESTAMP,
  `estado` enum('Confirmada','Cancelada','Completada','NoShow') DEFAULT 'Confirmada',
  `asistio` tinyint(1) DEFAULT NULL COMMENT 'Explicit flag for attendance - RF14',
  `notificacion_fin_enviada` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`id_reserva_maquina`),
  KEY `idx_maquina_tiempo` (`id_maquina`,`fecha_hora_inicio`,`fecha_hora_fin`),
  KEY `idx_usuario_tiempo` (`id_usuario`,`fecha_hora_inicio`),
  CONSTRAINT `reservasmaquinas_ibfk_1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE,
  CONSTRAINT `reservasmaquinas_ibfk_2` FOREIGN KEY (`id_maquina`) REFERENCES `maquinasejercicio` (`id_maquina`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reservasmaquinas`
--

LOCK TABLES `reservasmaquinas` WRITE;
/*!40000 ALTER TABLE `reservasmaquinas` DISABLE KEYS */;
INSERT INTO `reservasmaquinas` VALUES (1,2,1,'2025-04-24 15:46:49','2025-04-24 15:47:19','2025-04-23 15:35:49','Confirmada',NULL,0),(2,6,4,'2025-04-24 15:54:49','2025-04-24 15:55:34','2025-04-23 15:35:49','Confirmada',NULL,0),(3,9,1,'2025-04-26 15:51:49','2025-04-26 15:52:19','2025-04-23 15:35:49','Confirmada',NULL,0),(4,1,4,'2025-04-22 15:52:49','2025-04-22 15:53:34','2025-04-23 15:35:49','Completada',1,0),(5,2,1,'2025-04-20 15:42:49','2025-04-20 15:43:19','2025-04-23 15:35:49','Cancelada',NULL,0);
/*!40000 ALTER TABLE `reservasmaquinas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `id_rol` int NOT NULL AUTO_INCREMENT,
  `nombre_rol` varchar(50) NOT NULL COMMENT 'Ej: Cliente, Entrenador, Administrador, AdminGimnasio',
  PRIMARY KEY (`id_rol`),
  UNIQUE KEY `nombre_rol` (`nombre_rol`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (4,'AdminGimnasio'),(3,'Administrador'),(1,'Cliente'),(2,'Entrenador');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `rutinaejercicios`
--

DROP TABLE IF EXISTS `rutinaejercicios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rutinaejercicios` (
  `id_rutina_ejercicio` int NOT NULL AUTO_INCREMENT,
  `id_rutina` int NOT NULL,
  `id_ejercicio` int NOT NULL,
  `orden` int NOT NULL COMMENT 'Order of exercise in the routine',
  `series` varchar(20) DEFAULT NULL,
  `repeticiones` varchar(20) DEFAULT NULL,
  `descanso_segundos` int DEFAULT NULL,
  `notas_ejercicio` text,
  PRIMARY KEY (`id_rutina_ejercicio`),
  UNIQUE KEY `uk_rutina_ejercicio_orden` (`id_rutina`,`id_ejercicio`,`orden`),
  KEY `id_ejercicio` (`id_ejercicio`),
  CONSTRAINT `rutinaejercicios_ibfk_1` FOREIGN KEY (`id_rutina`) REFERENCES `rutinas` (`id_rutina`) ON DELETE CASCADE,
  CONSTRAINT `rutinaejercicios_ibfk_2` FOREIGN KEY (`id_ejercicio`) REFERENCES `ejercicios` (`id_ejercicio`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rutinaejercicios`
--

LOCK TABLES `rutinaejercicios` WRITE;
/*!40000 ALTER TABLE `rutinaejercicios` DISABLE KEYS */;
INSERT INTO `rutinaejercicios` VALUES (1,1,2,1,'3','8-10',90,'Foco en la técnica.'),(2,1,1,2,'3','8-10',90,NULL),(3,1,3,3,'3','10-12',75,'Mantener espalda recta.'),(4,2,1,1,'4','6-8',120,'Fase excéntrica controlada.'),(5,2,5,1,'4','10-12 por pierna',90,NULL);
/*!40000 ALTER TABLE `rutinaejercicios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `rutinas`
--

DROP TABLE IF EXISTS `rutinas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rutinas` (
  `id_rutina` int NOT NULL AUTO_INCREMENT,
  `id_entrenador_creador` int NOT NULL,
  `nombre_rutina` varchar(150) NOT NULL,
  `descripcion` text,
  `nivel` enum('Principiante','Intermedio','Avanzado') DEFAULT NULL,
  `objetivo` varchar(100) DEFAULT NULL,
  `fecha_creacion` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_rutina`),
  KEY `id_entrenador_creador` (`id_entrenador_creador`),
  CONSTRAINT `rutinas_ibfk_1` FOREIGN KEY (`id_entrenador_creador`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rutinas`
--

LOCK TABLES `rutinas` WRITE;
/*!40000 ALTER TABLE `rutinas` DISABLE KEYS */;
INSERT INTO `rutinas` VALUES (1,3,'Fuerza Básica 3 Días','Rutina de cuerpo completo enfocada en fuerza para principiantes.','Principiante','Fuerza','2025-04-13 15:36:37'),(2,7,'Hipertrofia Torso/Pierna','Rutina dividida para ganancia muscular intermedia.','Intermedio','Hipertrofia','2025-04-18 15:36:37'),(3,3,'Acondicionamiento Metabólico','Circuito de alta intensidad para mejorar resistencia.','Avanzado','Resistencia','2025-04-21 15:36:37'),(4,7,'Volumen Alemán Modificado','Rutina de alto volumen para hipertrofia avanzada.','Avanzado','Hipertrofia','2025-04-08 15:36:37'),(5,3,'Movilidad y Flexibilidad','Rutina corta para mejorar el rango de movimiento.','Principiante','Flexibilidad','2025-04-22 15:36:37');
/*!40000 ALTER TABLE `rutinas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tiposmembresia`
--

DROP TABLE IF EXISTS `tiposmembresia`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tiposmembresia` (
  `id_tipo_membresia` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(100) NOT NULL,
  `descripcion` text,
  `duracion_meses` int NOT NULL,
  `precio` decimal(10,2) NOT NULL,
  `activo` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`id_tipo_membresia`),
  UNIQUE KEY `nombre` (`nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tiposmembresia`
--

LOCK TABLES `tiposmembresia` WRITE;
/*!40000 ALTER TABLE `tiposmembresia` DISABLE KEYS */;
INSERT INTO `tiposmembresia` VALUES (1,'Mensual Básico','Acceso ilimitado a un gimnasio.',1,30.00,1),(2,'Mensual Premium','Acceso ilimitado a todos los gimnasios y clases online.',1,50.00,1),(3,'Anual Oro','Acceso total + 2 sesiones de entrenador personal al mes.',12,550.00,1),(4,'Plan Estudiante Semestral','Acceso básico a un gimnasio para estudiantes.',6,150.00,1),(5,'Pack 10 Visitas','Paquete de 10 accesos válidos por 3 meses.',3,80.00,0);
/*!40000 ALTER TABLE `tiposmembresia` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarioroles`
--

DROP TABLE IF EXISTS `usuarioroles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarioroles` (
  `id_usuario` int NOT NULL,
  `id_rol` int NOT NULL,
  PRIMARY KEY (`id_usuario`,`id_rol`),
  KEY `id_rol` (`id_rol`),
  CONSTRAINT `usuarioroles_ibfk_1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE,
  CONSTRAINT `usuarioroles_ibfk_2` FOREIGN KEY (`id_rol`) REFERENCES `roles` (`id_rol`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarioroles`
--

LOCK TABLES `usuarioroles` WRITE;
/*!40000 ALTER TABLE `usuarioroles` DISABLE KEYS */;
INSERT INTO `usuarioroles` VALUES (1,1),(2,1),(6,1),(9,1),(10,1),(3,2),(7,2),(5,3),(4,4),(8,4);
/*!40000 ALTER TABLE `usuarioroles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuariorutinas`
--

DROP TABLE IF EXISTS `usuariorutinas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuariorutinas` (
  `id_usuario_rutina` int NOT NULL AUTO_INCREMENT,
  `id_usuario` int NOT NULL,
  `id_rutina` int NOT NULL,
  `id_entrenador_asignador` int NOT NULL,
  `fecha_asignacion` datetime DEFAULT CURRENT_TIMESTAMP,
  `activa` tinyint(1) DEFAULT '1' COMMENT 'Is this the currently active routine for the user?',
  PRIMARY KEY (`id_usuario_rutina`),
  KEY `id_usuario` (`id_usuario`),
  KEY `id_rutina` (`id_rutina`),
  KEY `id_entrenador_asignador` (`id_entrenador_asignador`),
  CONSTRAINT `usuariorutinas_ibfk_1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE,
  CONSTRAINT `usuariorutinas_ibfk_2` FOREIGN KEY (`id_rutina`) REFERENCES `rutinas` (`id_rutina`) ON DELETE CASCADE,
  CONSTRAINT `usuariorutinas_ibfk_3` FOREIGN KEY (`id_entrenador_asignador`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuariorutinas`
--

LOCK TABLES `usuariorutinas` WRITE;
/*!40000 ALTER TABLE `usuariorutinas` DISABLE KEYS */;
INSERT INTO `usuariorutinas` VALUES (1,1,1,3,'2025-04-14 15:39:22',1),(2,2,2,7,'2025-04-19 15:39:22',1),(3,6,5,3,'2025-04-22 15:39:22',1),(4,1,3,3,'2025-04-22 15:39:22',0),(5,9,1,7,'2025-04-15 15:39:22',1);
/*!40000 ALTER TABLE `usuariorutinas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `id_usuario` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(100) DEFAULT NULL,
  `apellido` varchar(100) DEFAULT NULL,
  `email` varchar(100) NOT NULL,
  `password_hash` varchar(255) DEFAULT NULL,
  `fecha_registro` datetime DEFAULT CURRENT_TIMESTAMP,
  `activo` tinyint(1) DEFAULT '1',
  `foto_url` varchar(255) DEFAULT NULL,
  `fecha_nacimiento` date DEFAULT NULL,
  `genero` varchar(20) DEFAULT NULL,
  `direccion` varchar(255) DEFAULT NULL,
  `telefono` varchar(20) DEFAULT NULL,
  `oauth_provider` varchar(50) DEFAULT NULL COMMENT 'e.g., google, facebook',
  `oauth_id` varchar(100) DEFAULT NULL,
  `email_verificado` tinyint(1) DEFAULT '0',
  `token_verificacion_email` varchar(100) DEFAULT NULL,
  `fecha_expiracion_token` datetime DEFAULT NULL,
  `alertas_no_asistencia` int DEFAULT '0',
  `penalizado_hasta` datetime DEFAULT NULL,
  PRIMARY KEY (`id_usuario`),
  UNIQUE KEY `email` (`email`),
  UNIQUE KEY `uk_oauth` (`oauth_provider`,`oauth_id`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
INSERT INTO `usuarios` VALUES (1,'Juan','Pérez','juan.perez@email.com','$2y$10$...','2025-04-23 15:03:59',1,NULL,'1990-05-15','Masculino','Calle Luna 10','555-1111',NULL,NULL,1,NULL,NULL,0,NULL),(2,'Maria','García','maria.garcia@email.com','$2y$10$...','2025-04-23 15:03:59',1,NULL,'1995-02-20','Femenino','Avenida Sol 25','555-2222',NULL,NULL,1,NULL,NULL,0,NULL),(3,'Carlos','Martínez','carlos.entrenador@email.com','$2y$10$...','2025-04-23 15:03:59',1,NULL,'1988-11-30','Masculino','Plaza Estrella 5','555-3333',NULL,NULL,1,NULL,NULL,0,NULL),(4,'Ana','Rodríguez','ana.admin@email.com','$2y$10$...','2025-04-23 15:03:59',1,NULL,'1985-07-10','Femenino','Camino Real 100','555-4444',NULL,NULL,1,NULL,NULL,0,NULL),(5,'Luis','Fernández','luis.superadmin@email.com','$2y$10$...','2025-04-23 15:03:59',1,NULL,'1980-01-01','Masculino','Oficina Central 1','555-5555',NULL,NULL,1,NULL,NULL,0,NULL),(6,'Laura','López','laura.lopez@email.com','$2y$10$...','2025-04-23 15:03:59',1,NULL,'1998-09-05','Femenino','Calle Rio 8','555-6666',NULL,NULL,0,NULL,NULL,0,NULL),(7,'Pedro','Sánchez','pedro.trainer@email.com','$2y$10$...','2025-04-23 15:03:59',1,NULL,'1992-03-12','Masculino','Gimnasio Iron Body','555-7777',NULL,NULL,1,NULL,NULL,0,NULL),(8,'Sofia','Ramírez','sofia.gim@email.com','$2y$10$...','2025-04-23 15:03:59',1,NULL,'1991-12-22','Femenino','Gimnasio Zenith','555-8888',NULL,NULL,1,NULL,NULL,0,NULL),(9,'David','Gomez','david.google@email.com',NULL,'2025-04-23 15:03:59',1,NULL,'1993-06-18','Masculino','Calle Nueva 99','555-9999','google','google12345',1,NULL,NULL,0,NULL),(10,'Elena','Morales','elena.morales@email.com','$2y$10$...','2025-04-23 15:03:59',0,NULL,'1996-08-28','Femenino','Avenida Norte 30','555-1010',NULL,NULL,1,NULL,NULL,0,NULL),(11,NULL,NULL,'pepito@example.com','$2a$11$Fjfjcu/lEWBAGlnJWLT0MuceWv1AvvITUNaAb3SPIMti7I2ekD3.O','2025-04-23 18:46:14',1,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,NULL,NULL,0,NULL);
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-04-26 21:35:54
