<template>
  <div class="home card">
    <img class="home-bg" src="@/assets/images/welcome.png" alt="welcome" />
    <!-- 
     <div class="cardBig card">
      <div class="echarts">
        <ECharts :option="quantity" />
      </div>
    </div>
    -->
  </div>
</template>

<script setup lang="ts" name="home"></script>

<style scoped lang="scss">
.home {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  .home-bg {
    width: 70%;
    max-width: 1200px;
    margin-bottom: 20px;
  }
}
.cardList {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  width: 100%;
  .card {
    width: calc(50% - 5px);
    aspect-ratio: 1 / 0.5;
  }
}
.cardBig {
  width: 100%;
  margin-bottom: 10px;
}
.echarts {
  width: 100%;
  max-height: 40vh;
  aspect-ratio: 1 / 0.5;
}
</style>
