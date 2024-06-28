-- phpMyAdmin SQL Dump
-- version 5.0.4
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Jun 09, 2022 at 12:27 PM
-- Server version: 10.4.17-MariaDB
-- PHP Version: 7.4.13

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `memo_db`
--

-- --------------------------------------------------------

--
-- Table structure for table `attachment`
--

CREATE TABLE `attachment` (
  `I_ID` int(11) NOT NULL,
  `MEMO_ID` int(11) NOT NULL,
  `FILE_LOCATION` varchar(255) NOT NULL,
  `CREATED_BY` varchar(255) NOT NULL,
  `CREATED_TIME` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `attachment`
--

INSERT INTO `attachment` (`I_ID`, `MEMO_ID`, `FILE_LOCATION`, `CREATED_BY`, `CREATED_TIME`) VALUES
(1, 1, 'annexure_1.pdf', 'isurusampath@slt.com.lk', '2022-06-06 08:22:22'),
(2, 1, 'annex_2_for_memo1.doc', 'isurusampath@slt.com.lk', '2022-06-06 08:22:22'),
(3, 1, 'annex_3_for_memo1.doc', 'isurusampath@slt.com.lk', '2022-06-06 08:22:22'),
(4, 2, 'annex_1_for_memo2.doc', 'isurusampath@slt.com.lk', '2022-06-06 08:22:22'),
(5, 2, 'annex_2_for_memo2.doc', 'isurusampath@slt.com.lk', '2022-06-06 08:22:22'),
(6, 2, 'annex_3_for_memo2.doc', 'isurusampath@slt.com.lk', '2022-06-06 08:22:22'),
(7, 1, 'pri_data_2022_04_25.xls', 'isurusampath@slt.com.lk', '2022-06-09 03:33:55'),
(8, 1, 'ques.csv', 'isurusampath@slt.com.lk', '2022-06-09 03:33:55'),
(9, 1, 'error live.txt', 'isurusampath@slt.com.lk', '2022-06-09 03:54:53'),
(10, 1, 'workflow.sql', 'isurusampath@slt.com.lk', '2022-06-09 03:54:53');

-- --------------------------------------------------------

--
-- Table structure for table `forward`
--

CREATE TABLE `forward` (
  `I_ID` int(11) NOT NULL,
  `MEMO_ID` varchar(20) NOT NULL,
  `STAGE_ID` int(11) NOT NULL,
  `ASSIGNEE` varchar(255) NOT NULL,
  `ASSIGNER` varchar(255) NOT NULL,
  `STATUS` int(11) NOT NULL,
  `ASSIGNEE_COMMENT` varchar(255) NOT NULL,
  `ASSIGNER_COMMENT` varchar(255) NOT NULL,
  `ASSIGNEE_UPDATED_TIME` datetime NOT NULL,
  `ASSIGNER_UPDATED_TIME` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `forward`
--

INSERT INTO `forward` (`I_ID`, `MEMO_ID`, `STAGE_ID`, `ASSIGNEE`, `ASSIGNER`, `STATUS`, `ASSIGNEE_COMMENT`, `ASSIGNER_COMMENT`, `ASSIGNEE_UPDATED_TIME`, `ASSIGNER_UPDATED_TIME`) VALUES
(1, '1', 1, 'isurusampath@slt.com.lk', 'hiranyak@slt.com.lk', 2, 'efefefefefefefefefefef22222222222222', 'need to check this requirements', '0000-00-00 00:00:00', '2022-05-07 23:22:23');

-- --------------------------------------------------------

--
-- Table structure for table `memo`
--

CREATE TABLE `memo` (
  `ID` varchar(20) NOT NULL,
  `SUBJECT` varchar(255) NOT NULL,
  `DESCRIPTION` varchar(255) NOT NULL,
  `FILE_LOCATION` varchar(255) NOT NULL,
  `CREATED_BY` varchar(255) NOT NULL,
  `CREATED_TIME` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `memo`
--

INSERT INTO `memo` (`ID`, `SUBJECT`, `DESCRIPTION`, `FILE_LOCATION`, `CREATED_BY`, `CREATED_TIME`) VALUES
('1', 'memo 1', 'test memo 1', 'memo_1101.pdf', 'isurusampath@slt.com.lk', '2022-06-06 12:33:32'),
('2', 'memo 2', 'test memo 2', 'memo_two.pdf', 'isurusampath@slt.com.lk', '2022-06-03 12:33:32'),
('3', 'memo 3', 'test memo 3', 'memo_three.pdf', 'banuka@slt.com.lk', '2022-06-03 12:33:32');

-- --------------------------------------------------------

--
-- Table structure for table `stage`
--

CREATE TABLE `stage` (
  `I_ID` int(11) NOT NULL,
  `NAME` varchar(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `stage`
--

INSERT INTO `stage` (`I_ID`, `NAME`) VALUES
(1, 'recommend'),
(2, 'review'),
(3, 'approve'),
(4, 'execute'),
(5, 'create'),
(6, 'sign'),
(7, 'comment');

-- --------------------------------------------------------

--
-- Table structure for table `status`
--

CREATE TABLE `status` (
  `I_ID` int(11) NOT NULL,
  `NAME` varchar(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `status`
--

INSERT INTO `status` (`I_ID`, `NAME`) VALUES
(1, 'pending'),
(2, 'approved'),
(3, 'rejected'),
(4, 'forwarded'),
(5, 'created');

-- --------------------------------------------------------

--
-- Table structure for table `system_user`
--

CREATE TABLE `system_user` (
  `I_ID` int(11) NOT NULL,
  `EMAIL` varchar(255) NOT NULL,
  `PRI_ROLE` int(2) NOT NULL,
  `SERVICE_NO` varchar(6) NOT NULL,
  `NAME` varchar(255) NOT NULL,
  `DESIGNATION` varchar(255) NOT NULL,
  `SECTION` varchar(255) NOT NULL,
  `GROUP` varchar(255) NOT NULL,
  `CREATED_TIME` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `system_user`
--

INSERT INTO `system_user` (`I_ID`, `EMAIL`, `PRI_ROLE`, `SERVICE_NO`, `NAME`, `DESIGNATION`, `SECTION`, `GROUP`, `CREATED_TIME`) VALUES
(1, 'isurusampath@slt.com.lk', 1, '015751', 'Isuru Sampath', 'software developer', '', '', '2022-06-06 12:03:00'),
(2, 'hiranyak@slt.com.lk', 1, '015898', 'Hiranya Kuruppu', 'Engineer', '', '', '2022-06-06 12:03:00'),
(3, 'kosalat@slt.com.lk', 1, '012551', 'Kosala Tennakon', 'DGM', '', '', '2022-06-06 12:03:00');

-- --------------------------------------------------------

--
-- Table structure for table `work_progress`
--

CREATE TABLE `work_progress` (
  `I_ID` int(11) NOT NULL,
  `MEMO_ID` varchar(20) NOT NULL,
  `STAGE_ID` int(11) NOT NULL,
  `STAGE_NAME` int(11) NOT NULL,
  `ASSIGNEE` varchar(255) NOT NULL,
  `STATUS` int(11) NOT NULL,
  `COMMENT` varchar(255) NOT NULL,
  `UPDATED_TIME` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `work_progress`
--

INSERT INTO `work_progress` (`I_ID`, `MEMO_ID`, `STAGE_ID`, `STAGE_NAME`, `ASSIGNEE`, `STATUS`, `COMMENT`, `UPDATED_TIME`) VALUES
(1, '1', 1, 2, 'hiranyak@slt.com.lk', 4, 'need to approve this', '2022-06-06 12:22:33'),
(2, '1', 2, 3, 'kosalat@slt.com.lk', 1, 'need to approve this', '2022-06-06 12:22:33'),
(3, '2', 3, 2, 'hiranyak@slt.com.lk', 1, 'need to approve this', '2022-06-06 12:22:33'),
(4, '2', 4, 4, 'jana@slt.com.lk', 1, 'need to approve this', '2022-06-06 12:22:33'),
(5, '3', 1, 1, 'isurusampath@slt.com.lk', 1, 'need to approve this', '2022-06-02 12:02:33'),
(6, '2147483647', 1, 1, 'ighfhfhfgh@slt.com.lk', 1, '', '2022-06-08 03:37:49'),
(7, '2147483647', 2, 2, 'kumara@slt.com.lk', 1, '', '2022-06-08 03:38:14');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `attachment`
--
ALTER TABLE `attachment`
  ADD PRIMARY KEY (`I_ID`);

--
-- Indexes for table `forward`
--
ALTER TABLE `forward`
  ADD PRIMARY KEY (`I_ID`);

--
-- Indexes for table `memo`
--
ALTER TABLE `memo`
  ADD PRIMARY KEY (`ID`);

--
-- Indexes for table `stage`
--
ALTER TABLE `stage`
  ADD PRIMARY KEY (`I_ID`);

--
-- Indexes for table `status`
--
ALTER TABLE `status`
  ADD PRIMARY KEY (`I_ID`);

--
-- Indexes for table `system_user`
--
ALTER TABLE `system_user`
  ADD PRIMARY KEY (`I_ID`);

--
-- Indexes for table `work_progress`
--
ALTER TABLE `work_progress`
  ADD PRIMARY KEY (`I_ID`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `attachment`
--
ALTER TABLE `attachment`
  MODIFY `I_ID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `forward`
--
ALTER TABLE `forward`
  MODIFY `I_ID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT for table `stage`
--
ALTER TABLE `stage`
  MODIFY `I_ID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT for table `status`
--
ALTER TABLE `status`
  MODIFY `I_ID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `system_user`
--
ALTER TABLE `system_user`
  MODIFY `I_ID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `work_progress`
--
ALTER TABLE `work_progress`
  MODIFY `I_ID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
