CREATE DATABASE  IF NOT EXISTS `animalshelter` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `animalshelter`;
-- MySQL dump 10.13  Distrib 8.0.45, for Win64 (x86_64)
--
-- Host: localhost    Database: animalshelter
-- ------------------------------------------------------
-- Server version	8.0.45

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
-- Table structure for table `account`
--

DROP TABLE IF EXISTS `account`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `account` (
  `id` int NOT NULL AUTO_INCREMENT,
  `email` varchar(100) NOT NULL,
  `password` varchar(255) NOT NULL,
  `registrationDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `role` enum('user','shelter_admin','system_admin') NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `email` (`email`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `account`
--

LOCK TABLES `account` WRITE;
/*!40000 ALTER TABLE `account` DISABLE KEYS */;
INSERT INTO `account` VALUES (1,'karpiakmarta23@gmail.com','pass123','2026-03-14 00:15:32','user'),(2,'pzakutynskyi@gmail.com','pass123','2026-03-14 00:15:32','user'),(3,'haroqub@gmail.com','pass123','2026-03-14 00:15:32','user'),(4,'skorobogatuh.dara@gmail.com','pass123','2026-03-14 00:15:32','user'),(5,'diana.magotska@petly.com','pass123','2026-03-14 00:15:32','user'),(6,'happy.paws.lviv@gmail.com','pass123','2026-03-14 00:15:32','shelter_admin'),(7,'domivka.lviv@gmail.com','pass123','2026-03-14 00:15:32','shelter_admin'),(8,'animal.rescue.ua@gmail.com','pass123','2026-03-14 00:15:32','shelter_admin'),(9,'pets.home.kyiv@gmail.com','pass123','2026-03-14 00:15:32','shelter_admin'),(10,'admin@petly.com','admin123','2026-03-14 00:15:32','system_admin');
/*!40000 ALTER TABLE `account` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `adoptionapplication`
--

DROP TABLE IF EXISTS `adoptionapplication`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `adoptionapplication` (
  `adoptId` int NOT NULL AUTO_INCREMENT,
  `userId` int NOT NULL,
  `petId` int NOT NULL,
  `status` enum('Pending','Approved','Rejected') DEFAULT 'Pending',
  `submissionDate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`adoptId`),
  KEY `userId` (`userId`),
  KEY `petId` (`petId`),
  CONSTRAINT `adoptionapplication_ibfk_1` FOREIGN KEY (`userId`) REFERENCES `user` (`accountId`),
  CONSTRAINT `adoptionapplication_ibfk_2` FOREIGN KEY (`petId`) REFERENCES `pet` (`petId`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `adoptionapplication`
--

LOCK TABLES `adoptionapplication` WRITE;
/*!40000 ALTER TABLE `adoptionapplication` DISABLE KEYS */;
INSERT INTO `adoptionapplication` VALUES (6,1,21,'Pending','2026-03-14 00:35:17'),(7,2,24,'Approved','2026-03-14 00:35:17'),(8,3,27,'Pending','2026-03-14 00:35:17'),(9,4,23,'Rejected','2026-03-14 00:35:17'),(10,5,28,'Pending','2026-03-14 00:35:17');
/*!40000 ALTER TABLE `adoptionapplication` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `infopage`
--

DROP TABLE IF EXISTS `infopage`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `infopage` (
  `id` int NOT NULL AUTO_INCREMENT,
  `title` varchar(100) DEFAULT NULL,
  `content` text,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `infopage`
--

LOCK TABLES `infopage` WRITE;
/*!40000 ALTER TABLE `infopage` DISABLE KEYS */;
INSERT INTO `infopage` VALUES (1,'About Petly','Petly is a platform that connects animal shelters with people who want to adopt pets.'),(2,'Adoption Process','Browse animals, choose a pet and submit an adoption application. The shelter reviews the request.'),(3,'How to Help','You can help shelters by adopting pets, donating supplies.'),(4,'Contact','Contact shelters or platform administrator if you want to support animal rescue.');
/*!40000 ALTER TABLE `infopage` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pet`
--

DROP TABLE IF EXISTS `pet`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pet` (
  `petId` int NOT NULL AUTO_INCREMENT,
  `shelterId` int NOT NULL,
  `petName` varchar(100) NOT NULL,
  `type` enum('Dog','Cat','Other') NOT NULL,
  `breed` varchar(100) DEFAULT NULL,
  `gender` enum('Male','Female') DEFAULT NULL,
  `age` int DEFAULT NULL,
  `size` enum('Small','Medium','Large') DEFAULT NULL,
  `vaccinated` tinyint(1) DEFAULT '0',
  `sterilized` tinyint(1) DEFAULT '0',
  `status` enum('Available','Pending','Adopted') DEFAULT 'Available',
  `photoUrl` varchar(255) DEFAULT NULL,
  `description` text,
  `createdAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`petId`),
  KEY `shelterId` (`shelterId`),
  CONSTRAINT `pet_ibfk_1` FOREIGN KEY (`shelterId`) REFERENCES `shelter` (`accountId`)
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pet`
--

LOCK TABLES `pet` WRITE;
/*!40000 ALTER TABLE `pet` DISABLE KEYS */;
INSERT INTO `pet` VALUES (21,6,'Bella','Dog','Golden Retriever','Female',3,'Large',1,1,'Available','/images/pets/bella.jpg','Very friendly golden retriever','2026-03-14 00:32:08'),(22,6,'Max','Dog','German Shepherd','Male',5,'Large',1,1,'Available','/images/pets/max.jpg','Loyal and protective dog','2026-03-14 00:32:08'),(23,6,'Charlie','Dog','Labrador Retriever','Male',2,'Large',1,0,'Available','/images/pets/charlie.jpg','Energetic and playful','2026-03-14 00:32:08'),(24,7,'Luna','Cat','British Shorthair','Female',1,'Small',1,0,'Available','/images/pets/luna.jpg','Playful kitten','2026-03-14 00:32:08'),(25,7,'Milo','Cat','Maine Coon','Male',2,'Medium',1,1,'Available','/images/pets/milo.jpg','Calm and friendly cat','2026-03-14 00:32:08'),(26,7,'Simba','Cat','Mixed','Male',4,'Medium',1,1,'Available','/images/pets/simba.jpg','Very affectionate cat','2026-03-14 00:32:08'),(27,8,'Rocky','Dog','Mixed','Male',6,'Large',1,1,'Available','/images/pets/rocky.jpg','Rescued stray dog','2026-03-14 00:32:08'),(28,8,'Daisy','Dog','Beagle','Female',3,'Medium',1,1,'Available','/images/pets/daisy.jpg','Gentle and friendly dog','2026-03-14 00:32:08'),(29,9,'Oliver','Cat','Scottish Fold','Male',2,'Small',1,0,'Available','/images/pets/oliver.jpg','Curious cat','2026-03-14 00:32:08'),(30,9,'Lucy','Cat','Persian','Female',5,'Small',1,1,'Available','/images/pets/lucy.jpg','Very calm cat','2026-03-14 00:32:08');
/*!40000 ALTER TABLE `pet` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `shelter`
--

DROP TABLE IF EXISTS `shelter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `shelter` (
  `accountId` int NOT NULL,
  `shelterName` varchar(100) DEFAULT NULL,
  `location` varchar(150) DEFAULT NULL,
  `adminName` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`accountId`),
  CONSTRAINT `shelter_ibfk_1` FOREIGN KEY (`accountId`) REFERENCES `account` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `shelter`
--

LOCK TABLES `shelter` WRITE;
/*!40000 ALTER TABLE `shelter` DISABLE KEYS */;
INSERT INTO `shelter` VALUES (6,'Happy Paws Shelter','Lviv','Oksana Melnyk'),(7,'Domivka Shelter','Lviv','Iryna Koval'),(8,'Animal Rescue Ukraine','Kyiv','Serhii Bondarenko'),(9,'Pets Home Kyiv','Kyiv','Kateryna Hrytsenko');
/*!40000 ALTER TABLE `shelter` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `shelterneed`
--

DROP TABLE IF EXISTS `shelterneed`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `shelterneed` (
  `needId` int NOT NULL AUTO_INCREMENT,
  `shelterId` int NOT NULL,
  `description` text,
  `paymentDetails` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`needId`),
  KEY `shelterId` (`shelterId`),
  CONSTRAINT `shelterneed_ibfk_1` FOREIGN KEY (`shelterId`) REFERENCES `shelter` (`accountId`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `shelterneed`
--

LOCK TABLES `shelterneed` WRITE;
/*!40000 ALTER TABLE `shelterneed` DISABLE KEYS */;
INSERT INTO `shelterneed` VALUES (1,6,'Dry food and blankets needed for dogs','Bank transfer available'),(2,7,'Cat food and litter supplies needed','Donation box available at shelter'),(3,8,'Medical supplies needed for rescued animals','Veterinary support needed'),(4,9,'Funds needed for shelter renovation','Online donation available');
/*!40000 ALTER TABLE `shelterneed` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user` (
  `accountId` int NOT NULL,
  `name` varchar(100) DEFAULT NULL,
  `surname` varchar(100) DEFAULT NULL,
  `status` enum('Pending','Active','Rejected') DEFAULT 'Pending',
  PRIMARY KEY (`accountId`),
  CONSTRAINT `user_ibfk_1` FOREIGN KEY (`accountId`) REFERENCES `account` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user`
--

LOCK TABLES `user` WRITE;
/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user` VALUES (1,'Marta','Karpyak','Active'),(2,'Pavlo','Zakutynskyi','Active'),(3,'Nataliia','Ivanko','Active'),(4,'Daryna','Skorobagatykh','Active'),(5,'Diana','Mahotska','Active');
/*!40000 ALTER TABLE `user` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-14  0:46:40
