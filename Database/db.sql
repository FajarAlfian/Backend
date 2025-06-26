CREATE DATABASE Dlanguage;

USE Dlanguage;

-- Table ms_user
CREATE TABLE ms_user (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL,
    `password` VARCHAR(100) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Table ms_courses
CREATE TABLE ms_courses (
  course_id INT AUTO_INCREMENT PRIMARY KEY,
  course_name VARCHAR(255),
  course_price INT,
  category_id INT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Table ms_category
CREATE TABLE ms_category (
  category_id INT AUTO_INCREMENT PRIMARY KEY,
  category_name VARCHAR(255),
  category_description VARCHAR(255),
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Table tr_cart_product
CREATE TABLE tr_cart_product (
  cart_product_id INT AUTO_INCREMENT PRIMARY KEY,
  user_id INT,
  course_id INT,
  course_price INT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  CONSTRAINT fk_user
    FOREIGN KEY (user_id)
    REFERENCES ms_user(user_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);


-- Foreign key constraint for ms_courses referencing ms_category
ALTER TABLE ms_courses
ADD CONSTRAINT fk_category
FOREIGN KEY (category_id)
REFERENCES ms_category(category_id)
ON DELETE CASCADE
ON UPDATE CASCADE;