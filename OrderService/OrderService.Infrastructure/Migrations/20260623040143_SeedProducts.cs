using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                        table: "products",
                        columns: new[] { "Id", "Name", "UnitPrice", "AvailableStock" },
                        values: new object[,]
                        {
                { 1,  "Notebook Dell Inspiron",    3499.90m, 15  },
                { 2,  "Mouse Logitech MX Master",  349.90m,  80  },
                { 3,  "Teclado Mecânico Redragon",  299.90m,  60  },
                { 4,  "Monitor LG 24\" Full HD",   1199.90m, 25  },
                { 5,  "Headset HyperX Cloud II",   499.90m,  40  },
                { 6,  "Webcam Logitech C920",      599.90m,  30  },
                { 7,  "SSD Kingston 480GB",        229.90m,  100 },
                { 8,  "Memória RAM Corsair 16GB",  319.90m,  55  },
                { 9,  "Placa de Vídeo RTX 3060",  2199.90m, 8   },
                { 10, "Processador Ryzen 5 5600",  899.90m,  20  },
                { 11, "Gabinete Gamer NZXT H510",  699.90m,  12  },
                { 12, "Fonte Corsair 650W",        549.90m,  35  },
                { 13, "Placa-Mãe ASUS B550M",     749.90m,  18  },
                { 14, "Cooler DeepCool Gammaxx",   189.90m,  45  },
                { 15, "Mousepad XL Redragon",       89.90m,  120 },
                { 16, "Cabo HDMI 2.1 2m",           49.90m,  200 },
                { 17, "Hub USB-C 7 em 1",          179.90m,  50  },
                { 18, "Nobreak APC 700VA",         499.90m,  22  },
                { 19, "Suporte para Monitor",      149.90m,  70  },
                { 20, "Cadeira Gamer DXRacer",    1899.90m,  5   }
                        }
                    );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "products",
                keyColumn: "Id",
                keyValues: Enumerable.Range(1, 20).Cast<object>().ToArray()
            );
        }
    }
}
