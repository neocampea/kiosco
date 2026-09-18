-- Esquema de base de datos para StockVentas.
-- Cargar con: sudo mariadb stockventas < Database/schema.sql

CREATE TABLE IF NOT EXISTS usuarios (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    nombre_usuario  VARCHAR(50)  NOT NULL UNIQUE,
    nombre_completo VARCHAR(100) NOT NULL,
    rol             ENUM('Dueno','Empleado') NOT NULL,
    password_hash   VARCHAR(255) NOT NULL,
    activo          TINYINT(1)   NOT NULL DEFAULT 1,
    fecha_alta      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS productos (
    id           INT AUTO_INCREMENT PRIMARY KEY,
    nombre       VARCHAR(150) NOT NULL,
    descripcion  VARCHAR(500) NOT NULL DEFAULT '',
    categoria    VARCHAR(100) NOT NULL DEFAULT '',
    precio_base  DECIMAL(10,2) NOT NULL,
    stock        INT NOT NULL DEFAULT 0,
    activo       TINYINT(1) NOT NULL DEFAULT 1,
    fecha_alta   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ventas (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    fecha       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    medio_pago  ENUM('Efectivo','Transferencia','Debito','Credito') NOT NULL,
    estado      ENUM('Activa','Cancelada') NOT NULL DEFAULT 'Activa',
    usuario_id  INT NOT NULL,
    CONSTRAINT fk_ventas_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS items_venta (
    id               INT AUTO_INCREMENT PRIMARY KEY,
    venta_id         INT NOT NULL,
    producto_id      INT NOT NULL,
    nombre_producto  VARCHAR(150) NOT NULL,
    cantidad         INT NOT NULL,
    precio_unitario  DECIMAL(10,2) NOT NULL,
    CONSTRAINT fk_items_venta_venta    FOREIGN KEY (venta_id)    REFERENCES ventas(id)    ON DELETE CASCADE,
    CONSTRAINT fk_items_venta_producto FOREIGN KEY (producto_id) REFERENCES productos(id)
) ENGINE=InnoDB;

CREATE INDEX idx_ventas_usuario_fecha ON ventas(usuario_id, fecha);
