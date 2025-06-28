CREATE DATABASE Dlanguage;

USE Dlanguage;


-- ms_users
CREATE TABLE ms_user (
  user_id INT AUTO_INCREMENT PRIMARY KEY,
  username VARCHAR(255) NOT NULL,
  email VARCHAR(255) NOT NULL,
  PASSWORD VARCHAR(255) NOT NULL,
  ROLE ENUM('admin', 'member') NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ms_cart
CREATE TABLE ms_cart (
  cart_id INT AUTO_INCREMENT PRIMARY KEY,
  user_id INT NOT NULL,
  FOREIGN KEY (user_id) REFERENCES ms_user(user_id)
);

-- ms_profile
CREATE TABLE ms_profile (
  profile_id INT AUTO_INCREMENT PRIMARY KEY,
  full_name VARCHAR(255) NOT NULL,
  number_phone VARCHAR(30),
  user_id INT NOT NULL UNIQUE,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (user_id) REFERENCES ms_user(user_id)
);

-- ms_category
CREATE TABLE ms_category (
  category_id INT AUTO_INCREMENT PRIMARY KEY,
  category_name VARCHAR(255) NOT NULL,
  category_image VARCHAR(255),
  category_description TEXT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ms_courses
CREATE TABLE ms_courses (
  course_id INT AUTO_INCREMENT PRIMARY KEY,
  course_name VARCHAR(255) NOT NULL,
  course_price INT NOT NULL,
  course_image VARCHAR(255),
  course_description TEXT,
  category_id INT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (category_id) REFERENCES ms_category(category_id)
);

-- ms_schedule
CREATE TABLE ms_schedule (
  schedule_id INT AUTO_INCREMENT PRIMARY KEY,
  schedule_date DATETIME,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- tr_schedule_course
CREATE TABLE tr_schedule_course (
  schedule_course_id INT AUTO_INCREMENT PRIMARY KEY,
  course_id INT NOT NULL,
  schedule_id INT NOT NULL,
  FOREIGN KEY (course_id) REFERENCES ms_courses(course_id),
  FOREIGN KEY (schedule_id) REFERENCES ms_schedule(schedule_id),
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- tr_cart_product
CREATE TABLE tr_cart_product (
  cart_product_id INT AUTO_INCREMENT PRIMARY KEY,
  cart_id INT NOT NULL,
  course_id INT NOT NULL,
  schedule_course_id INT,
  course_price INT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (cart_id) REFERENCES ms_cart(cart_id),
  FOREIGN KEY (course_id) REFERENCES ms_courses(course_id),
  FOREIGN KEY (schedule_course_id) REFERENCES tr_schedule_course(schedule_course_id)
);

-- ms_payment_method
CREATE TABLE ms_payment_method (
  payment_method_id INT AUTO_INCREMENT PRIMARY KEY,
  payment_method_name VARCHAR(255) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- tr_invoice
CREATE TABLE tr_invoice (
  invoice_id INT AUTO_INCREMENT PRIMARY KEY,
  user_id INT,
  total_price INT,
  payment_method_id INT,
  isPaid TINYINT(1) DEFAULT 0,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (user_id) REFERENCES ms_user(user_id),
  FOREIGN KEY (payment_method_id) REFERENCES ms_payment_method(payment_method_id)
);

-- tr_invoice_detail
CREATE TABLE tr_invoice_detail (
  invoice_detail_id INT AUTO_INCREMENT PRIMARY KEY,
  invoice_id INT,
  cart_product_id INT,
  sub_total_price INT,
  FOREIGN KEY (invoice_id) REFERENCES tr_invoice(invoice_id),
  FOREIGN KEY (cart_product_id) REFERENCES tr_cart_product(cart_product_id)
);
