
# 🛒 Gestión de Productos

**Gestión de Productos** es una aplicación de consola desarrollada en C# (.NET 8.0) que permite gestionar productos de manera eficiente. Ideal para pequeñas empresas, estudiantes o desarrolladores que buscan una solución práctica y funcional para el control de inventarios.

## 🚀 Características

- **CRUD de productos**: Crea, lee, actualiza y elimina productos fácilmente.
- **Interfaz de consola amigable**: Navegación sencilla mediante menús interactivos.
- **Arquitectura modular**: Separación clara entre datos, modelos y lógica de negocio.
- **Persistencia de datos**: Almacena la información de productos para su uso posterior.

## 🧱 Estructura del Proyecto

```
Gestion-Productos/
├── Datos/           # Manejo de datos y almacenamiento
├── Modelos/         # Definición de clases y estructuras de datos
├── Modulos/         # Lógica de negocio y operaciones
├── Program.cs       # Punto de entrada de la aplicación
├── GestionProducto.csproj
└── .gitignore
```

## 🛠️ Requisitos

- .NET SDK 8.0 o superior
- Sistema operativo compatible (Windows, Linux, macOS)

## ⚙️ Instalación y Ejecución

1. Clona el repositorio:
   ```bash
   git clone https://github.com/matttyggDEsing/Gestion-Productos.git
   cd Gestion-Productos
   ```

2. Compila el proyecto:
   ```bash
   dotnet build
   ```

3. Ejecuta la aplicación:
   ```bash
   dotnet run
   ```

## 🎯 Uso

Al iniciar la aplicación, se presentará un menú interactivo en la consola que permite:

- Agregar nuevos productos.
- Listar productos existentes.
- Actualizar información de productos.
- Eliminar productos del inventario.

Sigue las instrucciones en pantalla para navegar por las opciones y gestionar tus productos de manera eficiente.

## 🤝 Contribuciones

¡Las contribuciones son bienvenidas! Si deseas mejorar este proyecto, por favor sigue estos pasos:

1. Haz un fork del repositorio.
2. Crea una nueva rama para tu funcionalidad o corrección:
   ```bash
   git checkout -b nombre-de-tu-rama
   ```

3. Realiza tus cambios y haz commit:
   ```bash
   git commit -m "Descripción de tus cambios"
   ```

4. Haz push a tu rama:
   ```bash
   git push origin nombre-de-tu-rama
   ```

5. Abre un Pull Request en GitHub.

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Consulta el archivo `LICENSE` para más detalles.
