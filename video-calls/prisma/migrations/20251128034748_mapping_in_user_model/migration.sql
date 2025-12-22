/*
  Warnings:

  - You are about to drop the column `twoFactorEnabled` on the `users` table. All the data in the column will be lost.

*/
-- AlterTable
ALTER TABLE "users" DROP COLUMN "twoFactorEnabled",
ADD COLUMN     "two_factors_enabled" BOOLEAN DEFAULT false;
