using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PublicShareOwnerControl.Migrations
{
    public partial class AddedIdToShareholder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Shareholder",
                table: "Shareholder");

            migrationBuilder.RenameColumn(
                name: "ShareholderId",
                table: "Stocks",
                newName: "Id");

            migrationBuilder.AddColumn<Guid>(
                name: "StockOwner",
                table: "Stocks",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "Shareholder",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Shareholder",
                table: "Shareholder",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Shareholder",
                table: "Shareholder");

            migrationBuilder.DropColumn(
                name: "StockOwner",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Shareholder");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Stocks",
                newName: "ShareholderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Shareholder",
                table: "Shareholder",
                column: "ShareholderId");
        }
    }
}
