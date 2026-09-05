CREATE DATABASE IF NOT EXISTS course_registration;
USE course_registration;

CREATE TABLE IF NOT EXISTS courses (
  name VARCHAR(20) PRIMARY KEY,
  title VARCHAR(200) NOT NULL,
  credits DECIMAL(3, 1) NOT NULL,
  description VARCHAR(2000) NOT NULL,
  CONSTRAINT chk_courses_credits CHECK (credits BETWEEN 0.5 AND 12.0)
);

CREATE TABLE IF NOT EXISTS core_goals (
  id VARCHAR(20) PRIMARY KEY,
  name VARCHAR(200) NOT NULL,
  description VARCHAR(2000) NOT NULL
);

CREATE TABLE IF NOT EXISTS core_goal_courses (
  goal_id VARCHAR(20) NOT NULL,
  course_name VARCHAR(20) NOT NULL,
  PRIMARY KEY (goal_id, course_name),
  CONSTRAINT fk_goal_courses_goal
    FOREIGN KEY (goal_id) REFERENCES core_goals (id) ON DELETE CASCADE,
  CONSTRAINT fk_goal_courses_course
    FOREIGN KEY (course_name) REFERENCES courses (name) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS course_offerings (
  course_name VARCHAR(20) NOT NULL,
  semester VARCHAR(50) NOT NULL,
  section VARCHAR(20) NOT NULL,
  PRIMARY KEY (course_name, semester, section),
  CONSTRAINT fk_offerings_course
    FOREIGN KEY (course_name) REFERENCES courses (name) ON DELETE CASCADE
);

INSERT IGNORE INTO courses (name, title, credits, description) VALUES
  ('CSCI 330', 'Software Engineering', 3.0, 'Software design, construction, testing, and maintenance.'),
  ('CSCI 434', 'Digital Forensics', 3.0, 'Methods and tools used to investigate digital evidence.'),
  ('ENGL 102', 'Composition and Critical Reading', 3.0, 'Research, argumentation, and academic writing.');

INSERT IGNORE INTO core_goals (id, name, description) VALUES
  ('CG1', 'Critical Thinking', 'Analyze information and develop evidence-based conclusions.');

INSERT IGNORE INTO core_goal_courses (goal_id, course_name) VALUES
  ('CG1', 'CSCI 330'),
  ('CG1', 'ENGL 102');

INSERT IGNORE INTO course_offerings (course_name, semester, section) VALUES
  ('CSCI 330', 'Spring 2026', '01'),
  ('CSCI 434', 'Spring 2026', '01'),
  ('ENGL 102', 'Spring 2026', '04');

