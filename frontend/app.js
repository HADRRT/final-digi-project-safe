const API = 'http://localhost:8081/api';
let token = '';
let role = '';
let childName = '';
let allExpenses = [];

// ---- LOGIN ----
async function login() {
    const name = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    if (!name || !password) {
        document.getElementById('loginError').textContent = 'Please enter username and password!';
        return;
    }

    try {
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

            document.getElementById('loginPage').classList.remove('active');

            if (role === 'Child') {
                document.getElementById('childPage').classList.add('active');
                document.getElementById('childUsername').textContent = `👶 ${name}`;
                loadChildExpenses();
            } else {
                document.getElementById('supervisorPage').classList.add('active');
                document.getElementById('supervisorUsername').textContent = `👨‍💼 ${name}`;
                loadExpenses();
            }
        } else {
            document.getElementById('loginError').textContent = 'Invalid username or password!';
        }
    } catch (err) {
        document.getElementById('loginError').textContent = 'Cannot connect to server!';
    }
}

// ---- LOGOUT ----
function logout() {
    token = '';
    role = '';
    childName = '';
    allExpenses = [];

    document.getElementById('childPage').classList.remove('active');
    document.getElementById('supervisorPage').classList.remove('active');
    document.getElementById('loginPage').classList.add('active');
    document.getElementById('username').value = '';
    document.getElementById('password').value = '';
    document.getElementById('loginError').textContent = '';
}

// ---- ADD EXPENSE (Child) ----
async function addExpense() {
    const reason = document.getElementById('reason').value;
    const amount = document.getElementById('amount').value;

    if (!reason || !amount) {
        document.getElementById('childMessage').innerHTML =
            '<p class="error-msg">Please fill all fields!</p>';
        return;
    }

    try {
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
                '<p class="success-msg">✅ Expense submitted successfully!</p>';
            document.getElementById('reason').value = '';
            document.getElementById('amount').value = '';
            loadChildExpenses();

            setTimeout(() => {
                document.getElementById('childMessage').innerHTML = '';
            }, 3000);
        }
    } catch (err) {
        document.getElementById('childMessage').innerHTML =
            '<p class="error-msg">Error submitting expense!</p>';
    }
}

// ---- LOAD CHILD EXPENSES ----
async function loadChildExpenses() {
    try {
        const res = await fetch(`${API}/Expenses/my`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (res.ok) {
            const expenses = await res.json();
            renderChildExpenses(expenses);
        }
    } catch (err) {
        console.log('Error loading expenses');
    }
}

function renderChildExpenses(expenses) {
    const list = document.getElementById('childExpensesList');

    if (expenses.length === 0) {
        list.innerHTML = '<div class="empty-state">📭 No expenses yet</div>';
        return;
    }

    list.innerHTML = expenses.map(exp => `
        <div class="expense-card ${exp.status.toLowerCase()}">
            <div class="expense-header">
                <span class="expense-name">${exp.reason}</span>
                <span class="expense-amount">${exp.amount} EGP</span>
            </div>
            <span class="status-badge ${exp.status.toLowerCase()}">${exp.status}</span>
        </div>
    `).join('');
}

// ---- LOAD ALL EXPENSES (Supervisor) ----
async function loadExpenses() {
    try {
        const res = await fetch(`${API}/Expenses`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (res.ok) {
            allExpenses = await res.json();
            updateStats();
            renderExpenses(allExpenses);
        }
    } catch (err) {
        console.log('Error loading expenses');
    }
}

function updateStats() {
    document.getElementById('totalCount').textContent = allExpenses.length;
    document.getElementById('pendingCount').textContent = allExpenses.filter(e => e.status === 'Pending').length;
    document.getElementById('approvedCount').textContent = allExpenses.filter(e => e.status === 'Approved').length;
    document.getElementById('rejectedCount').textContent = allExpenses.filter(e => e.status === 'Rejected').length;
}

function filterExpenses(status) {
    document.querySelectorAll('.filter-btn').forEach(btn => btn.classList.remove('active'));
    event.target.classList.add('active');

    const filtered = status === 'All' ? allExpenses : allExpenses.filter(e => e.status === status);
    renderExpenses(filtered);
}

function renderExpenses(expenses) {
    const list = document.getElementById('expensesList');

    if (expenses.length === 0) {
        list.innerHTML = '<div class="empty-state">📭 No expenses found</div>';
        return;
    }

    list.innerHTML = expenses.map(exp => `
        <div class="expense-card ${exp.status.toLowerCase()}">
            <div class="expense-header">
                <span class="expense-name">👶 ${exp.childName}</span>
                <span class="expense-amount">${exp.amount} EGP</span>
            </div>
            <p class="expense-reason">📝 ${exp.reason}</p>
            <span class="status-badge ${exp.status.toLowerCase()}">${exp.status}</span>
            ${exp.status === 'Pending' ? `
                <div class="expense-actions">
                    <button class="btn-approve" onclick="approve(${exp.id})">✅ Approve</button>
                    <button class="btn-reject" onclick="reject(${exp.id})">❌ Reject</button>
                </div>
            ` : ''}
        </div>
    `).join('');
}

// ---- APPROVE / REJECT ----
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

// ---- ENTER KEY LOGIN ----
document.addEventListener('keypress', function(e) {
    if (e.key === 'Enter') login();
});