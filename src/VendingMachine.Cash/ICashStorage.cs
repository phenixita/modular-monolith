namespace VendingMachine.Cash;

public interface ICashStorage
{
    decimal GetBalance();

    void SetBalance(decimal balance);

    void EnsureCreated();
}
