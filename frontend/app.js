const API = 'http://localhost:8081/api';
let token = '';
let role = '';
let childName = '';

async function login() {
    const name = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    const res = await fetch(`${API}/Auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, password })
    });

    if (res.ok) {
        const data = await res.json();
        token = data.token;
        role = data.role;
        childName = name;

        document.getElementById('loginPage').style.display = 'none';

        if (role === 'Child') {
            document.getElementById('childPage').style.display = 'block';
        } else {
            document.getElementById('supervisorPage').style.display = 'block';
            loadExpenses();
        }
    } else {
        document.getElementById('loginError').textContent = 'Invalid credentials!';
    }
}

async function addExpense() {
    const reason = document.getElementById('reason').value;
    const amount = document.getElementById('amount').value;

    const res = await fetch(`${API}/Expenses`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ reason, amount: parseFloat(amount), childName })
    });

    if (res.ok) {
        document.getElementById('childMessage').innerHTML = 
            '<p style="color:green">✅ Expense added successfully!</p>';
        document.getElementById('reason').value = '';
        document.getElementById('amount').value = '';
    }
}

async function loadExpenses() {
    const res = await fetch(`${API}/Expenses`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });

    const expenses = await res.json();
    const list = document.getElementById('expensesList');
    list.innerHTML = '';

    expenses.forEach(exp => {
        list.innerHTML += `
            <div class="expense-card ${exp.status.toLowerCase()}">
                <p><strong>${exp.childName}</strong></p>
                <p>Reason: ${exp.reason}</p>
                <p>Amount: ${exp.amount} EGP</p>
                <p>Status: <strong>${exp.status}</strong></p>
                ${exp.status === 'Pending' ? `
                    <button class="approve-btn" onclick="approve(${exp.id})">✅ Approve</button>
                    <button class="reject-btn" onclick="reject(${exp.id})">❌ Reject</button>
                ` : ''}
            </div>
        `;
    });
}

async function approve(id) {
    await fetch(`${API}/Expenses/${id}/approve`, {
        method: 'PUT',
        headers: { 'Authorization': `Bearer ${token}` }
    });
    loadExpenses();
}

async function reject(id) {
    await fetch(`${API}/Expenses/${id}/reject`, {
        method: 'PUT',
        headers: { 'Authorization': `Bearer ${token}` }
    });
    loadExpenses();
}