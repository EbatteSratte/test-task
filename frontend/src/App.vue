<template>
  <div id="app">
    <header class="header">
      <h1>Управление документами</h1>
      <nav class="nav-tabs">
        <router-link to="/documents" class="nav-tab" active-class="active">
          📄 Документы
        </router-link>
        <router-link to="/logs" class="nav-tab" active-class="active">
          📋 Логи
        </router-link>
      </nav>
    </header>
    
    <main class="main-content">
      <router-view />
    </main>
  </div>
</template>

<script>
export default {
  name: 'App'
}
</script>