<template>
	<view class="page" v-if="bannerObj['10003']">
		<view class="headerBox">
			<!-- 我的收益 -->
			<view class="profitBox">
				<image :src="bannerObj['10003'].imgUrl" mode="widthFix" style="width: 100%;"></image>
				<view class="box">
					<view class="left">
						<view class="num">我的收益</view>
						<view class="price">{{ wallet.amount.toFixed(2) }}</view>
					</view>
					<view class="btn" @click="href('/pagesPerson/withdraw')">提现</view>
				</view>
			</view>
			<view class="block">
				<view class="item">
					<view class="num">{{ wallet.dayTotal }}</view>
					<view class="text">今日收益(元)</view>
				</view>
				<view class="item">
					<view class="num">{{ wallet.yesterdayTotal }}</view>
					<view class="text">昨日收益(元)</view>
				</view>
			</view>
		</view>
		<view class="body">
			<view class="pageTitle">
				<view class="name">收入明细</view>
				<view class="time" @click="show = true">
					<view class="text">{{ queryYearMonth }}</view>
					<up-icon name="arrow-down"></up-icon>
				</view>
			</view>
			<view class="square" v-if="list.length > 0" v-for="item in list" :key="item">
				<view class="left">
					<view class="name">{{ item.title }} ( {{ item.remark || "无备注" }} )</view>
					<view class="time">{{ item.createTime }}</view>
				</view>
				<view class="right">{{ item.change }}</view>
			</view>
			<view class="emptyBlock" v-else>
				<image src="/static/img32.png" mode="widthFix"></image>
			</view>
			<view style="height: 24rpx;"></view>
		</view>
		<up-datetime-picker :show="show" @cancel="show = false" @close="show = false" v-model="timeValue"
			:maxDate="Number(new Date()) + 1" @confirm="selectDate" mode="year-month"
			closeOnClickOverlay></up-datetime-picker>
	</view>
</template>

<script>
export default {
	data() {
		return {
			bannerObj: {},
			show: false,
			queryYearMonth: 0,
			timeValue: Number(new Date()) - 1,
			wallet: {
				amount:''
			},
			list: "",
			total: 0,
			isEye: false,
			query: {
				state: 1,
				queryType: 0,
				pageNum: 1,
				pageSize: 10,
				queryYear: "",
				queryMonth: "",
			},
		}
	},
	onLoad(options) {
		// 时间初始化赋值
		let dateTime = this.timeYearMonth(Number(new Date()) - 1)
		this.queryYearMonth = dateTime[0] + '-' + dateTime[1]
		this.query.queryYear = dateTime[0]
		this.query.queryMonth = dateTime[1]
		this.getMyWallet()
		this.getLoadWithdrawalList()
		this.request(`news/BannerList`, {
			Ids: [10003]
		}).then(res => {
			this.bannerObj = res.data.items
		})
	},
	onShow() {
		this.getMyWallet()
		this.getLoadWithdrawalList()
	},
	async onReachBottom() {
		if (this.total > this.query.pageNum * this.query.pageSize) {
			this.query.pageNum += 1
			await this.getLoadWithdrawalList()
		}
	},
	methods: {
		async getMyWallet() {
			let res = await this.request('Money/MyWallet', {
				queryType: 0,
			})
			this.wallet = res.data
			console.log(this.wallet);
		},
		async getLoadWithdrawalList(reset) {
			if (reset) this.query.pageNum = 1
			let res = await this.request('Money/MoneyList', {
				...this.query,
			})
			if (this.query.pageNum == 1) {
				this.list = res.data.items
				this.total = res.data.total
			} else {
				this.list.push(...res.data.items)
			}
		},
		// 日期
		selectDate(e) {
			let dateTime = this.timeYearMonth(e.value)
			this.queryYearMonth = dateTime[0] + '-' + dateTime[1]
			this.query.queryYear = dateTime[0]
			this.query.queryMonth = dateTime[1]
			this.getLoadWithdrawalList(true)
			this.show = false
		},
	}
}
</script>

<style lang="scss" scoped>
.page {
	position: relative;

	.headerBox {
		padding: 32rpx;


		.profitBox {
			position: relative;

			.box {
				display: flex;
				justify-content: space-between;
				align-items: center;
				height: 100%;
				position: absolute;
				width: 100%;
				left: 0;
				top: 0;
				padding: 0 40rpx;

				.left {
					.num {
						font-size: 24rpx;
						color: #FFFFFF;
					}

					.price {
						margin-top: 14rpx;
						font-weight: 600;
						font-size: 48rpx;
						color: #FFFFFF;
					}
				}

				.btn {
					width: 152rpx;
					height: 64rpx;
					background: #FFFFFF;
					border-radius: 16rpx;
					display: flex;
					justify-content: center;
					align-items: center;
					font-size: 28rpx;
					color: #8C4FFF;
				}
			}
		}

		.block {
			display: flex;
			justify-content: space-between;
			align-items: center;
			margin-top: 24rpx;

			.item {
				display: flex;
				flex-direction: column;
				align-items: center;
				justify-content: center;
				background: #FFFFFF;
				border-radius: 16rpx; 
				width: 100%;
				padding: 24rpx 0;
				.num {
					font-weight: 600;
					font-size: 32rpx;
					color: #333333;
				}
	
				.text {
					font-size: 24rpx;
					color: #333333;
					margin-top: 8rpx;
				}
			}
			.item:not(:last-child) {
				margin-right: 26rpx;
			}
		}
	}

	.body {
		border-radius: 24rpx;
		margin-top: 24rpx;

		.pageTitle {
			display: flex;
			justify-content: space-between;
			padding: 26rpx 32rpx;

			.name {
				font-weight: 600;
				font-size: 32rpx;
				color: #333333;
			}

			.time {
				display: flex;
				align-items: center;
				font-size: 36rpx;
				color: #000000;

				.text {
					margin-right: 16rpx;
				}
			}
		}

		.square {
			display: flex;
			align-items: center;
			justify-content: space-between;
			background: #FFFFFF;
			border-radius: 8rpx;
			width: 686rpx;
			padding: 32rpx;
			margin: 0 auto;
			margin-bottom: 24rpx;

			.left {
				.name {
					font-size: 30rpx;
					color: #333333;
				}

				.time {
					margin-top: 13rpx;
					font-size: 24rpx;
					color: #666666;
				}
			}

			.right {
				font-size: 30rpx;
				color: #8C4FFF;
			}
		}
	}
}
</style>