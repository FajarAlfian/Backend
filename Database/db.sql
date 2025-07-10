CREATE DATABASE Dlanguage;
USE Dlanguage;

-- ms_user
CREATE TABLE ms_user (
  user_id INT AUTO_INCREMENT PRIMARY KEY,
  username VARCHAR(255) NOT NULL,
  email VARCHAR(255) NOT NULL,
  PASSWORD VARCHAR(255) NOT NULL,
  ROLE ENUM('admin', 'member') NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
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
  schedule_date VARCHAR(100),
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
  user_id INT NOT NULL,
  course_id INT NOT NULL,
  schedule_course_id INT,
  course_price INT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (user_id) REFERENCES ms_user(user_id),
  FOREIGN KEY (course_id) REFERENCES ms_courses(course_id),
  FOREIGN KEY (schedule_course_id) REFERENCES tr_schedule_course(schedule_course_id)
);

-- ms_payment_method
CREATE TABLE ms_payment_method (
  payment_method_id INT AUTO_INCREMENT PRIMARY KEY,
  payment_method_name VARCHAR(255) NOT NULL,
  payment_method_logo VARCHAR(255) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- tr_invoice
CREATE TABLE tr_invoice (
  invoice_id INT AUTO_INCREMENT PRIMARY KEY,
  invoice_number VARCHAR(50) UNIQUE,
  user_id INT,
  total_price INT,
  payment_method_id INT,
  isPaid TINYINT(1) DEFAULT 0,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (user_id) REFERENCES ms_user(user_id),
  FOREIGN KEY (payment_method_id) REFERENCES ms_payment_method(payment_method_id)
);

-- tr_invoice_detail
CREATE TABLE tr_invoice_detail (
  invoice_detail_id INT AUTO_INCREMENT PRIMARY KEY,
  invoice_id INT,
  cart_product_id INT NULL,
  course_id INT,
  sub_total_price INT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (invoice_id) REFERENCES tr_invoice(invoice_id),
  FOREIGN KEY (cart_product_id) REFERENCES tr_cart_product(cart_product_id) ON DELETE SET NULL,
  FOREIGN KEY (course_id) REFERENCES ms_courses(course_id)
);

-- Add foreign key constraint to tr_invoice_detail
ALTER TABLE tr_invoice_detail
ADD CONSTRAINT fk_invoice_detail_cart_product
FOREIGN KEY (cart_product_id) REFERENCES tr_cart_product(cart_product_id)
ON DELETE RESTRICT;

-- Add PasswordResetToken and PasswordResetTokenCreatedAt to ms_user
ALTER TABLE ms_user
ADD COLUMN PasswordResetToken VARCHAR(255) NULL AFTER PASSWORD,
ADD COLUMN PasswordResetTokenCreatedAt DATETIME NULL AFTER PasswordResetToken;

-- Add is_verified, email_verification_token, and email_token_created_at columns to ms_user
ALTER TABLE ms_user
ADD COLUMN is_verified TINYINT(1) NOT NULL,
ADD COLUMN email_verification_token VARCHAR(255),
ADD COLUMN email_token_created_at DATETIME;

ALTER TABLE tr_invoice_detail
ADD COLUMN schedule_course_id INT(11) NULL AFTER cart_product_id;

ALTER TABLE ms_user
ADD COLUMN is_deleted TINYINT(1) NOT NULL DEFAULT 0 AFTER is_verified;

ALTER TABLE ms_courses
ADD COLUMN is_active TINYINT(1) NOT NULL DEFAULT 0
AFTER category_id;


-- Insert data into ms_category
INSERT INTO ms_category (category_name, category_image, category_description, created_at, updated_at) VALUES
('Arabic', "https://flagcdn.com/w320/sa.png", 'Kursus Bahasa Arab dirancang untuk membekali peserta dengan kemampuan membaca, menulis, mendengarkan, dan berbicara dalam bahasa Arab secara efektif. Bahasa Arab merupakan salah satu bahasa resmi dunia dan digunakan secara luas di Timur Tengah, Afrika Utara, serta negara-negara Islam. Kursus ini membahas mulai dari dasar huruf dan angka Arab, tata bahasa (nahwu dan sharaf), percakapan sehari-hari, hingga pemahaman teks Al-Quran dan literatur Arab modern. Dengan metode pembelajaran interaktif, siswa diajak untuk praktik dialog, mendengarkan native speaker, serta memahami kebudayaan Arab yang kaya. Kursus ini sangat cocok bagi pemula, pelajar, maupun profesional yang ingin meningkatkan kompetensi komunikasi atau persiapan studi dan bisnis di negara-negara Arab.', NOW(), NOW()),
('Deutsch', "https://flagcdn.com/w320/de.png", 'Kursus Bahasa Jerman (Deutsch) menawarkan pembelajaran menyeluruh tentang tata bahasa, kosakata, pelafalan, dan keterampilan komunikasi dalam bahasa Jerman. Bahasa Jerman adalah salah satu bahasa penting di Eropa, menjadi bahasa resmi di Jerman, Austria, Swiss, dan beberapa negara lain. Materi kursus meliputi pemahaman teks, percakapan sehari-hari, latihan menulis surat, serta diskusi budaya Jerman. Melalui metode praktis dan latihan bersama, peserta didorong untuk aktif berbicara, memahami instruksi, serta membangun kepercayaan diri dalam menggunakan bahasa Jerman untuk studi, pekerjaan, atau perjalanan ke negara-negara berbahasa Jerman. Kursus ini terbuka bagi pemula hingga tingkat lanjutan, serta sangat bermanfaat bagi mereka yang tertarik pada budaya, teknologi, dan pendidikan di Jerman.', NOW(), NOW()),
('English', "https://flagcdn.com/w320/gb.png", 'Kursus Bahasa Inggris adalah solusi ideal bagi siapa saja yang ingin menguasai bahasa internasional ini, baik untuk keperluan akademik, bisnis, maupun kehidupan sehari-hari. Program ini mencakup empat keterampilan utama: listening, speaking, reading, dan writing. Materi disusun sistematis mulai dari level dasar hingga mahir, mencakup grammar, vocabulary, pronunciation, serta latihan percakapan dengan situasi nyata. Pengajaran dilakukan secara interaktif dengan metode diskusi, presentasi, serta simulasi situasi kehidupan sehari-hari dan profesional. Selain itu, kursus ini juga memberikan tips menghadapi tes TOEFL/IELTS bagi yang ingin studi atau bekerja di luar negeri. Didukung tutor berpengalaman, kursus ini akan membantu Anda percaya diri berkomunikasi dalam bahasa Inggris.', NOW(), NOW()),
('French', "https://flagcdn.com/w320/fr.png", 'Kursus Bahasa Prancis memberikan pengenalan serta pengembangan keterampilan berbahasa Prancis untuk keperluan akademik, bisnis, maupun wisata. Bahasa Prancis adalah salah satu bahasa internasional yang digunakan di banyak negara di dunia. Materi kursus meliputi tata bahasa dasar, pengucapan, kosakata, serta latihan mendengarkan dan berbicara dalam berbagai situasi sehari-hari. Peserta juga akan belajar menulis teks sederhana, memahami budaya Prancis, dan melakukan percakapan dengan native speaker. Kursus ini sangat cocok bagi pemula maupun yang ingin memperdalam kemampuan bahasa Prancis, terutama untuk persiapan studi di Prancis, bekerja di perusahaan multinasional, atau sekadar menambah keterampilan bahasa asing.', NOW(), NOW()),
('Indonesian', "https://flagcdn.com/w320/id.png", 'Kursus Bahasa Indonesia dirancang untuk membantu peserta dari berbagai latar belakang menguasai bahasa Indonesia secara efektif. Program ini meliputi tata bahasa, kosakata, serta latihan berbicara, mendengarkan, membaca, dan menulis. Melalui latihan percakapan sehari-hari dan pemahaman budaya Indonesia, peserta dapat lebih mudah beradaptasi dengan lingkungan sosial dan profesional di Indonesia. Materi kursus juga mencakup idiom, peribahasa, serta pembahasan gaya bahasa formal dan informal yang sering digunakan dalam percakapan dan tulisan resmi. Kursus ini sangat bermanfaat untuk ekspatriat, pelajar asing, atau siapa saja yang ingin memperdalam pemahaman bahasa Indonesia untuk kehidupan, studi, atau bisnis di Indonesia.', NOW(), NOW()),
('Japanese', "https://flagcdn.com/w320/jp.png", 'Kursus Bahasa Jepang menawarkan pembelajaran sistematis mulai dari pengenalan huruf Hiragana, Katakana, hingga Kanji, serta tata bahasa dasar sampai lanjutan. Kursus ini dirancang untuk memudahkan peserta dalam memahami percakapan sehari-hari, membaca teks, serta menulis dalam bahasa Jepang. Melalui latihan berbicara, mendengarkan audio native speaker, dan pengetahuan budaya Jepang, peserta dapat meningkatkan kepercayaan diri dalam berkomunikasi. Materi kursus juga meliputi etika, kebiasaan, dan situasi sosial di Jepang, sehingga sangat bermanfaat bagi pelajar, profesional, atau wisatawan yang ingin berinteraksi secara efektif di lingkungan berbahasa Jepang.', NOW(), NOW()),
('Mandarin', "https://flagcdn.com/w320/cn.png", 'Kursus Bahasa Mandarin berfokus pada penguasaan keterampilan dasar seperti mendengarkan, berbicara, membaca, dan menulis karakter Hanzi. Bahasa Mandarin merupakan bahasa dengan penutur terbanyak di dunia dan menjadi kunci komunikasi di Tiongkok dan komunitas internasional. Peserta akan belajar fonetik (pinyin), kosakata, tata bahasa, serta penggunaan bahasa dalam percakapan sehari-hari dan situasi bisnis. Dengan pendekatan interaktif, kursus ini juga mengenalkan budaya dan kebiasaan masyarakat Tiongkok. Kursus sangat bermanfaat bagi pelajar, pekerja, maupun pelaku bisnis yang ingin memperluas jejaring dan peluang di negara-negara berbahasa Mandarin.', NOW(), NOW()),
('Melayu', "https://flagcdn.com/w320/my.png", 'Kursus Bahasa Melayu bertujuan untuk memberikan pemahaman mendalam tentang bahasa dan budaya Melayu yang digunakan di Indonesia, Malaysia, Brunei, dan Singapura. Materi kursus meliputi tata bahasa, kosakata, pengucapan, serta latihan membaca dan berbicara. Peserta juga akan memahami perbedaan dialek dan penggunaan bahasa Melayu dalam berbagai situasi sosial dan profesional. Kursus ini sangat cocok untuk pelajar, pekerja, maupun siapa saja yang ingin memperluas kemampuan komunikasi di kawasan Asia Tenggara. Selain itu, kursus ini juga membahas sejarah dan tradisi Melayu, sehingga peserta dapat lebih mengenal identitas dan kearifan lokal budaya Melayu.', NOW(), NOW());


-- Insert data into ms_courses
INSERT INTO ms_courses (course_name, course_price, course_image, course_description, category_id)
VALUES
('Basic English for Junior', 400000, 'https://res.cloudinary.com/ddd8hwouh/image/upload/v1751101847/english_junior_gxxdeu.png',
'Basic English for Junior merupakan program kursus bahasa Inggris yang didesain khusus untuk anak-anak usia sekolah dasar hingga remaja. Materi dalam kursus ini meliputi pembelajaran kosa kata dasar, struktur kalimat sederhana, serta latihan berbicara dan mendengarkan yang interaktif. Program ini dirancang agar siswa dapat berlatih berbicara dalam bahasa Inggris secara aktif melalui permainan, lagu, dan aktivitas menyenangkan lainnya. Selain itu, kursus ini juga membantu membangun kepercayaan diri anak dalam menggunakan bahasa Inggris di lingkungan sehari-hari. Dengan pengajar berpengalaman dan metode pembelajaran modern, kursus ini sangat cocok untuk anak-anak yang ingin memiliki fondasi bahasa Inggris yang kuat dan percaya diri dalam berkomunikasi.', 
3),
('Complit Package - Expert English, TOEFL and IELT', 2000000, 'https://res.cloudinary.com/ddd8hwouh/image/upload/v1751101847/english_expert_ti4uzg.png',
'Complit Package - Expert English, TOEFL and IELT adalah paket kursus komprehensif yang dirancang untuk membantu peserta mencapai tingkat keahlian tinggi dalam bahasa Inggris. Program ini mencakup persiapan ujian TOEFL dan IELTS, latihan listening, reading, writing, dan speaking, serta strategi khusus untuk menghadapi berbagai jenis soal ujian internasional. Dengan materi yang terstruktur dan bimbingan dari instruktur berpengalaman, peserta akan memperoleh keterampilan akademik dan komunikasi yang sangat dibutuhkan dalam lingkungan global, studi ke luar negeri, maupun dunia kerja. Kursus ini sangat direkomendasikan untuk pelajar, mahasiswa, atau profesional yang ingin memperoleh sertifikat bahasa Inggris dengan skor tinggi.',
3),
('Level 1 Mandarin', 200000, 'https://res.cloudinary.com/ddd8hwouh/image/upload/v1751101850/mandarin_zt0vbt.png',
'Level 1 Mandarin adalah kursus bahasa Mandarin tingkat dasar yang dirancang untuk pemula tanpa latar belakang bahasa Mandarin sebelumnya. Dalam kursus ini, peserta akan mempelajari pengenalan karakter Hanzi, tata bahasa dasar, serta latihan berbicara dan mendengarkan untuk kebutuhan sehari-hari. Program ini juga memberikan pemahaman tentang budaya dan kebiasaan masyarakat Tiongkok. Dengan metode pembelajaran interaktif dan bimbingan dari instruktur profesional, peserta akan mampu memperkenalkan diri, melakukan percakapan sederhana, serta memahami dasar komunikasi dalam bahasa Mandarin, sehingga menjadi bekal untuk melanjutkan ke level berikutnya.',
7),
('Arabic Course - Beginner to Middle', 550000, 'https://res.cloudinary.com/ddd8hwouh/image/upload/v1751101846/arabic_iyb8lp.png',
'Arabic Course - Beginner to Middle adalah program kursus yang cocok untuk siapa saja yang ingin belajar bahasa Arab dari dasar hingga tingkat menengah. Materi kursus meliputi pengenalan huruf Arab, kosa kata sehari-hari, serta latihan membaca, menulis, dan berbicara. Peserta juga akan mempelajari tata bahasa Arab yang sederhana hingga menengah dan dibimbing dalam praktik percakapan sehari-hari sesuai situasi nyata. Dengan dukungan pengajar berpengalaman, kursus ini membantu peserta memahami bahasa Arab dengan cara yang mudah dan menyenangkan, serta menumbuhkan kepercayaan diri dalam berkomunikasi di lingkungan Arab.',
1),
('Kursus Bahasa Indonesia', 650000, 'https://res.cloudinary.com/ddd8hwouh/image/upload/v1751101847/indonesia_zdi3n4.png',
'Kursus Bahasa Indonesia adalah program belajar bahasa Indonesia yang diperuntukkan bagi penutur asing atau siapa saja yang ingin memperdalam keterampilan bahasa Indonesia secara efektif. Materi kursus mencakup tata bahasa, kosa kata, ekspresi sehari-hari, serta latihan berbicara, menulis, membaca, dan mendengarkan. Dengan metode pengajaran komunikatif dan interaktif, peserta akan lebih mudah memahami budaya dan kebiasaan masyarakat Indonesia. Kursus ini sangat bermanfaat untuk pelajar asing, ekspatriat, atau profesional yang membutuhkan kemampuan bahasa Indonesia untuk studi, bekerja, atau beraktivitas di Indonesia.',
5),
('Germany Language for Junior', 450000, 'https://res.cloudinary.com/ddd8hwouh/image/upload/v1751101847/germany_uodzhm.png',
'Germany Language for Junior adalah kursus bahasa Jerman yang dirancang khusus untuk anak-anak dan remaja. Program ini mengajarkan dasar-dasar bahasa Jerman, seperti kosa kata sehari-hari, pengucapan, serta tata bahasa yang mudah dipahami. Melalui aktivitas interaktif dan latihan berbicara, siswa akan mampu berkomunikasi secara sederhana dalam bahasa Jerman. Kursus ini juga membekali peserta dengan pengetahuan budaya Jerman, permainan edukatif, dan pembelajaran visual yang menarik. Sangat cocok bagi anak-anak yang ingin mengenal bahasa asing baru dan membangun fondasi yang kuat untuk jenjang pendidikan berikutnya.',
2);

-- Insert default payment into ms_payment_method
INSERT INTO ms_payment_method (payment_method_name, payment_method_logo) VALUES 
("Gopay","https://res.cloudinary.com/dllo4dtar/image/upload/v1751325284/gopay_qyfauu.png"),
("OVO","https://res.cloudinary.com/dllo4dtar/image/upload/v1751325284/ovo_qbpieq.jpg"),
("Dana","https://res.cloudinary.com/dllo4dtar/image/upload/v1751325284/dana_rfzti7.jpg"),
("Mandiri","https://res.cloudinary.com/dllo4dtar/image/upload/v1751325283/mandiri_h8bdrl.png"),
("BCA","https://res.cloudinary.com/dllo4dtar/image/upload/v1751325283/bca_mrhl0t.svg"),
("BNI","https://res.cloudinary.com/dllo4dtar/image/upload/v1751325285/bni_zmffs5.png");

--insert default date
INSERT INTO ms_schedule (schedule_date) VALUES
("2025-07-21"),
("2025-07-22"),
("2025-07-23"),
("2025-07-24"),
("2025-07-25"),
("2025-07-26"),
("2025-07-27");

-- add banner
ALTER TABLE ms_category ADD COLUMN category_banner VARCHAR(255) AFTER category_description;

UPDATE ms_category
SET category_banner = 'https://res.cloudinary.com/ddd8hwouh/image/upload/v1751933810/Screenshot_2025-07-08_071443_fdqm97.png'
WHERE category_id = 1;

UPDATE ms_category
SET category_banner = 'https://res.cloudinary.com/ddd8hwouh/image/upload/v1751933940/Screenshot_2025-07-08_071840_chneqc.png'
WHERE category_id = 2;

UPDATE ms_category
SET category_banner = 'https://res.cloudinary.com/ddd8hwouh/image/upload/v1751933952/Screenshot_2025-07-08_070523_enkvre.png'
WHERE category_id = 3;

UPDATE ms_category
SET category_banner = 'https://res.cloudinary.com/ddd8hwouh/image/upload/v1751934103/Screenshot_2025-07-08_071954_emoxte.png'
WHERE category_id = 4;

UPDATE ms_category
SET category_banner = 'https://res.cloudinary.com/ddd8hwouh/image/upload/v1751934112/Screenshot_2025-07-08_072037_fqkrm2.png'
WHERE category_id = 5;

UPDATE ms_category
SET category_banner = 'https://res.cloudinary.com/ddd8hwouh/image/upload/v1751934113/Screenshot_2025-07-08_072108_btqzyq.png'
WHERE category_id = 6;

UPDATE ms_category
SET category_banner = 'https://res.cloudinary.com/ddd8hwouh/image/upload/v1751934113/Screenshot_2025-07-08_072127_cs0hgt.png'
WHERE category_id = 7;

UPDATE ms_category
SET category_banner = 'https://res.cloudinary.com/ddd8hwouh/image/upload/v1751934667/Screenshot_2025-07-08_073057_kf9rti.png'
WHERE category_id = 8;

-- add is_active
ALTER TABLE ms_payment_method
ADD COLUMN is_active TINYINT(1) NOT NULL DEFAULT 0
AFTER payment_method_id;

UPDATE ms_payment_method
SET is_active = 1
WHERE payment_method_id = 1;

UPDATE ms_payment_method
SET is_active = 1
WHERE payment_method_id = 2;

UPDATE ms_payment_method
SET is_active = 1
WHERE payment_method_id = 3;

UPDATE ms_payment_method
SET is_active = 1
WHERE payment_method_id = 4;

UPDATE ms_payment_method
SET is_active = 1
WHERE payment_method_id = 5;

UPDATE ms_payment_method
SET is_active = 1
WHERE payment_method_id = 6;

-- add is_active
ALTER TABLE ms_category
ADD COLUMN is_active TINYINT(1) NOT NULL DEFAULT 0
AFTER category_banner;

UPDATE ms_category
SET is_active = 1
WHERE category_id = 1;

UPDATE ms_category
SET is_active = 1
WHERE category_id = 2;

UPDATE ms_category
SET is_active = 1
WHERE category_id = 3;

UPDATE ms_category
SET is_active = 1
WHERE category_id = 4;

UPDATE ms_category
SET is_active = 1
WHERE category_id = 5;

UPDATE ms_category
SET is_active = 1
WHERE category_id = 6;

UPDATE ms_category
SET is_active = 1
WHERE category_id = 7; 

UPDATE ms_category
SET is_active = 1
WHERE category_id = 8;