﻿using StockManagemant.Entities.DTO;
using StockManagemant.DataAccess.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.Business.Managers
{
    public interface IReceiptManager
    {
        Task<int> GetTotalReceiptCountAsync(ReceiptFilter filter);
        Task<List<ReceiptDto>> GetPagedReceiptAsync(int page, int pageSize, ReceiptFilter filter);
        Task<int> AddReceiptAsync(ReceiptDto receiptDto);
        Task UpdateReceiptDateAsync(ReceiptDto updateDto);
        Task UpdateReceiptAsync(int receiptId);
        Task DeleteReceiptAsync(int receiptId);
        Task<ReceiptDto> GetReceiptByIdAsync(int receiptId);
        Task<List<ReceiptDetailDto>> GetReceiptDetailsAsync(int receiptId);
        Task RemoveProductFromReceiptAsync(int receiptDetailId);
        Task UpdateProductQuantityInReceiptAsync(int receiptDetailId, int newQuantity);
    }
}